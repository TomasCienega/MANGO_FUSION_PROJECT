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
    public class OrderHeaderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ApiResponse _response;

        public OrderHeaderController(ApplicationDbContext context, ApiResponse response)
        {
            _context = context;
            _response = response;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetOrders(string userId="")
        {
            try
            {
                IQueryable<OrderHeader> orderHeaderList = _context.OrderHeaders.Include(u => u.OrderDetails)
                    .ThenInclude(u => u.MenuItem).OrderByDescending(u => u.OrderHeaderId);
                if (!string.IsNullOrEmpty(userId))
                {
                    orderHeaderList = orderHeaderList.Where(u => u.ApplicationUserId == userId);
                }
                _response.Result = orderHeaderList;
                _response.StatusCode = HttpStatusCode.OK;
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

        [HttpGet("{orderId:int}")]
        public async Task<ActionResult<ApiResponse>> GetOrder(int orderId)
        {
            try
            {
                if (orderId==0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessage.Add("Invalid order Id");
                    return BadRequest(_response);
                }
                OrderHeader? orderHeader = await _context.OrderHeaders.Include(u => u.OrderDetails)
                    .ThenInclude(u => u.MenuItem).FirstOrDefaultAsync(u=> u.OrderHeaderId==orderId);

                if (orderHeader == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessage.Add("Order not Found");
                    return NotFound(_response);
                }

                _response.Result = orderHeader;
                _response.StatusCode = HttpStatusCode.OK;
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

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateOrder([FromBody] OrderHeaderCreateDTO orderHeaderDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    OrderHeader orderHeader = new()
                    {
                        PickUpName = orderHeaderDTO.PickUpName,
                        PickUpPhoneNumber = orderHeaderDTO.PickUpPhoneNumber,
                        PickUpEmail = orderHeaderDTO.PickUpEmail,
                        OrderDate = DateTime.UtcNow,
                        OrderTotal = orderHeaderDTO.OrderTotal,
                        Status = StaticDetails.Status_Confirmed,
                        TotalItem = orderHeaderDTO.TotalItem,
                        ApplicationUserId = orderHeaderDTO.ApplicationUserId,

                    };
                    await _context.OrderHeaders.AddAsync(orderHeader);
                    await _context.SaveChangesAsync();

                    foreach (var orderDetailDTO in orderHeaderDTO.OrderDetailsDTO)
                    {
                        OrderDetail orderDetail = new()
                        {
                            OrderHeaderId = orderHeader.OrderHeaderId,
                            MenuItemId = orderDetailDTO.MenuItemId,
                            Quantity = orderDetailDTO.Quantity,
                            ItemName = orderDetailDTO.ItemName,
                            Price = orderDetailDTO.Price,
                        };
                        await _context.OrderDetails.AddAsync(orderDetail);
                    }
                    await _context.SaveChangesAsync();
                    _response.Result = orderHeader;
                    orderHeader.OrderDetails = [];
                    _response.StatusCode = HttpStatusCode.Created;
                    return CreatedAtAction(nameof(GetOrder), new { orderId = orderHeader.OrderHeaderId }, _response);
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

        [HttpPut("{orderId:int}")]
        public async Task<ActionResult<ApiResponse>> UpdateOrder(int orderId,[FromBody] OrderHeaderUpdateDTO orderHeaderDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (orderId!= orderHeaderDTO.OrderHeaderId)
                    {
                        _response.IsSuccess = false;
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.ErrorMessage.Add("Invalid Id");
                        return BadRequest(_response);
                    }
                    OrderHeader? orderHeaderFromDb = await _context.OrderHeaders.FirstOrDefaultAsync(o => o.OrderHeaderId == orderId);

                    if (orderHeaderFromDb == null)
                    {
                        _response.IsSuccess = false;
                        _response.StatusCode = HttpStatusCode.NotFound;
                        _response.ErrorMessage.Add("Order Not Found");
                        return NotFound(_response);
                    }

                    if (!string.IsNullOrEmpty(orderHeaderDTO.PickUpName))
                    {
                        orderHeaderFromDb.PickUpName = orderHeaderDTO.PickUpName;
                    }
                    if (!string.IsNullOrEmpty(orderHeaderDTO.PickUpPhoneNumber))
                    {
                        orderHeaderFromDb.PickUpPhoneNumber = orderHeaderDTO.PickUpPhoneNumber;
                    }
                    if (!string.IsNullOrEmpty(orderHeaderDTO.PickUpEmail))
                    {
                        orderHeaderFromDb.PickUpEmail = orderHeaderDTO.PickUpEmail;
                    }
                    if (!string.IsNullOrEmpty(orderHeaderDTO.Status))
                    {
                        if(orderHeaderFromDb.Status.Equals(StaticDetails.Status_Confirmed, StringComparison.InvariantCultureIgnoreCase) 
                            && orderHeaderDTO.Status.Equals(StaticDetails.Status_ReadyForPickUp, StringComparison.InvariantCultureIgnoreCase))
                        {
                            orderHeaderFromDb.Status = StaticDetails.Status_ReadyForPickUp;
                        }
                        if (orderHeaderFromDb.Status.Equals(StaticDetails.Status_ReadyForPickUp, StringComparison.InvariantCultureIgnoreCase)
                            && orderHeaderDTO.Status.Equals(StaticDetails.Status_Completed, StringComparison.InvariantCultureIgnoreCase))
                        {
                            orderHeaderFromDb.Status = StaticDetails.Status_Completed;
                        }
                        if (orderHeaderDTO.Status.Equals(StaticDetails.Status_Cancelled, StringComparison.InvariantCultureIgnoreCase))
                        {
                            orderHeaderFromDb.Status = StaticDetails.Status_Cancelled;
                        }
                    }
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
