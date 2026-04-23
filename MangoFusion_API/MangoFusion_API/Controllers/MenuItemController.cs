using MangoFusion_API.Data;
using MangoFusion_API.Models;
using MangoFusion_API.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MangoFusion_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuItemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ApiResponse _response;
        private readonly IWebHostEnvironment _env;
        public MenuItemController(ApplicationDbContext context, ApiResponse response, IWebHostEnvironment env)
        {
            _context = context;
            _response = response;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetItemsMenu()
        {
            try
            {
                var items = await _context.MenuItems.ToListAsync();
                var orderRatings = await _context.OrderDetails.Where(u => u.Rating != null).ToListAsync();
                if (items != null && items.Any())
                {
                    List<MenuItem> menuItems = items;
                    List<OrderDetail> orderDetailsWithRatings = orderRatings;

                    foreach (var menuItem in menuItems)
                    {
                        var ratings = orderDetailsWithRatings.Where(u => u.MenuItemId == menuItem.Id).Select(u => u.Rating!.Value);
                        double avgRating = ratings.Any() ? ratings.Average() : 0;
                        menuItem.Rating = avgRating;
                    }
                    _response.Result = menuItems;
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    return Ok(_response);
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessage = ["No se encontraron elementos en el Menú"];
                    return NotFound(_response);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessage = [ex.Message];
                return StatusCode(500, _response);
            }
        }

        [HttpGet("{id:int}", Name = "GetMenuItem")]
        public async Task<IActionResult> GetMenuItemById(int id)
        {
            try
            {
                if (id==0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessage.Add("Invalid Id");
                    return BadRequest(_response);
                }


                MenuItem? item = await _context.MenuItems.FindAsync(id);
                if (item == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessage = ["El elemento con el ID " + id + " no existe."];
                    return NotFound(_response);

                }
                var averageRating = await _context.OrderDetails
                    .Where(u => u.MenuItemId == id && u.Rating != null)
                    .AverageAsync(u => (double?)u.Rating) ?? 0;

                item.Rating = averageRating;

                _response.Result = item;
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessage = [ex.Message];
                return StatusCode(500, _response);
            }
        }

        #region
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> CreateMenuItem([FromForm] MenuItemCreateDTO _createDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // 1. Validación de archivo
                    if (_createDTO.File == null || _createDTO.File.Length == 0)
                    {
                        _response.IsSuccess = false;
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.ErrorMessage = ["File is required"];
                        return BadRequest(_response);
                    }

                    // 2. Preparar rutas
                    var _imagePath = Path.Combine(_env.WebRootPath, "images");
                    if (!Directory.Exists(_imagePath))
                    {
                        Directory.CreateDirectory(_imagePath);
                    }

                    // 3. Generar nombre único con GUID
                    var extension = Path.GetExtension(_createDTO.File.FileName);
                    var nombreUnico = Guid.NewGuid().ToString() + extension;
                    var _filePath = Path.Combine(_imagePath, nombreUnico);

                    // 4. Guardar archivo físicamente
                    using (var stream = new FileStream(_filePath, FileMode.Create))
                    {
                        await _createDTO.File.CopyToAsync(stream);
                    }

                    // 5. Mapear al Modelo de Base de Datos
                    var _menuItem = new MenuItem()
                    {
                        Name = _createDTO.Name,
                        Description = _createDTO.Description,
                        Price = _createDTO.Price,
                        Category = _createDTO.Category,
                        SpecialTag = _createDTO.SpecialTag,
                        Image = "images/" + nombreUnico // <-- Usamos el GUID directamente
                    };

                    // 6. Guardar en Base de Datos
                    await _context.MenuItems.AddAsync(_menuItem);
                    await _context.SaveChangesAsync();

                    // 7. Respuesta Exitosa
                    _response.Result = _menuItem; // Devolvemos el objeto creado con su ID
                    _response.StatusCode = HttpStatusCode.Created;
                    _response.IsSuccess = true; // No olvides marcar el éxito

                    return CreatedAtRoute("GetMenuItem", new { id = _menuItem.Id }, _response);
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessage = ["Modelo no válido"];
                    return BadRequest(_response);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessage = [ex.Message];
                return StatusCode(500, _response);
            }

        }
        #endregion

        [HttpPut("{id:int}")] // Es mejor recibir el ID en la URL
        public async Task<ActionResult<ApiResponse>> UpdateMenuItem(int id, [FromForm] MenuItemUpdateDTO _updateDTO)
        {
            try
            {
                // Validación inicial: Que el modelo sea válido y los IDs coincidan
                if (!ModelState.IsValid || _updateDTO.Id != id)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessage = ["IDs no coinciden o modelo inválido"];
                    return BadRequest(_response);
                }

                var menuItemFromDB = await _context.MenuItems.FirstOrDefaultAsync(u => u.Id == id);

                if (menuItemFromDB == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessage = ["No fue posible mostrar el objeto"];
                    return NotFound(_response);
                }

                // Actualizamos datos de texto
                menuItemFromDB.Name = _updateDTO.Name;
                menuItemFromDB.Description = _updateDTO.Description;
                menuItemFromDB.Price = _updateDTO.Price;
                menuItemFromDB.Category = _updateDTO.Category;
                menuItemFromDB.SpecialTag = _updateDTO.SpecialTag;

                // ¿Viene una nueva imagen?
                if (_updateDTO.File != null && _updateDTO.File.Length > 0)
                {
                    var _imagePath = Path.Combine(_env.WebRootPath, "images");

                    // 1. Borrar la imagen vieja físicamente
                    if (!string.IsNullOrEmpty(menuItemFromDB.Image))
                    {
                        var _oldFilePath = Path.Combine(_env.WebRootPath, menuItemFromDB.Image);
                        if (System.IO.File.Exists(_oldFilePath))
                        {
                            System.IO.File.Delete(_oldFilePath);
                        }
                    }

                    // 2. Generar nombre nuevo con GUID (Igual que en el Create)
                    var extension = Path.GetExtension(_updateDTO.File.FileName).ToLower();
                    var nombreUnico = Guid.NewGuid().ToString() + extension;
                    var _newFilePath = Path.Combine(_imagePath, nombreUnico);

                    // 3. Guardar nueva imagen
                    using (var stream = new FileStream(_newFilePath, FileMode.Create))
                    {
                        await _updateDTO.File.CopyToAsync(stream);
                    }

                    // 4. Actualizar la ruta en la DB
                    menuItemFromDB.Image = "images/" + nombreUnico;
                }

                _context.MenuItems.Update(menuItemFromDB);
                await _context.SaveChangesAsync();

                _response.StatusCode = HttpStatusCode.OK; // Cambiado de NoContent a OK para devolver el objeto
                _response.IsSuccess = true;
                _response.Result = menuItemFromDB;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessage = [ex.Message];
                return StatusCode(500, _response);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse>> DeleteMenuItem(int id)
        {
            try
            {
                // Validación inicial: Que el modelo sea válido y los IDs coincidan
                if (id == 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessage = ["IDs no existe"];
                    return BadRequest(_response);
                }

                var menuItemFromDB = await _context.MenuItems.FirstOrDefaultAsync(u => u.Id == id);

                if (menuItemFromDB == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessage = ["No fue posible mostrar el objeto"];
                    return NotFound(_response);
                }


                if (!string.IsNullOrEmpty(menuItemFromDB.Image))
                {
                    var _oldFilePath = Path.Combine(_env.WebRootPath, menuItemFromDB.Image);
                    if (System.IO.File.Exists(_oldFilePath))
                    {
                        System.IO.File.Delete(_oldFilePath);
                    }
                }

                _context.MenuItems.Remove(menuItemFromDB);
                await _context.SaveChangesAsync();

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = menuItemFromDB;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessage = [ex.Message];
                return StatusCode(500, _response);
            }
        }
    }
}
