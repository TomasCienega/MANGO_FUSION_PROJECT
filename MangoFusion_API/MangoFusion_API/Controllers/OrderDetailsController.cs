using MangoFusion_API.Data;
using MangoFusion_API.Models;
using MangoFusion_API.Models.Dto;
using MangoFusion_API.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MangoFusion_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ApiResponse _response;

        public OrderDetailsController(ApplicationDbContext context, ApiResponse response)
        {
            _context = context;
            _response = response;
        }

        [HttpPut("{orderDetailsId:int}")]
        public async Task<ActionResult<ApiResponse>> UpdateOrder(int orderDetailsId, [FromBody] OrderDetailsUpdateDTO orderDetailsDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (orderDetailsId != orderDetailsDTO.OrderDetailId)
                    {
                        _response.IsSuccess = false;
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.ErrorMessage.Add("Invalid Id");
                        return BadRequest(_response);
                    }
                    OrderDetail? orderDetailFromDb = await _context.OrderDetails.FirstOrDefaultAsync(o => o.OrderDetailId == orderDetailsId);

                    if (orderDetailFromDb == null)
                    {
                        _response.IsSuccess = false;
                        _response.StatusCode = HttpStatusCode.NotFound;
                        _response.ErrorMessage.Add("Order Not Found");
                        return NotFound(_response);
                    }
                    orderDetailFromDb.Rating = orderDetailsDTO.Rating;
                    await _context.SaveChangesAsync();

                    _response.StatusCode = HttpStatusCode.OK;
                    return Ok(_response);
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessage = ModelState.Values.SelectMany(u => u.Errors).Select(u => u.ErrorMessage).ToList();
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
    }
}
