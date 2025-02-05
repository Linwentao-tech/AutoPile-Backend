using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;
using AutoPile.DOMAIN.Models;
using AutoPile.SERVICE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoPile.API.Controllers
{
    /// <summary>
    /// Controller for managing orders in the AutoPile system
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        /// <summary>
        /// Initializes a new instance of the OrderController
        /// </summary>
        /// <param name="orderService">The order service dependency</param>
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Creates a new order for the authenticated user
        /// </summary>
        /// <param name="orderCreateDTO">The order details to create</param>
        /// <returns>The created order information</returns>
        /// <response code="200">Returns the newly created order</response>
        /// <response code="400">If the order details are invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpPost("CreateOrder", Name = "CreateOrder")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDTO orderCreateDTO)
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            var orderResponse = await _orderService.CreateOrderAsync(orderCreateDTO, userId);
            return ApiResponse<OrderResponseDTO>.OkResult(orderResponse);
        }

        /// <summary>
        /// Retrieves a specific order by its ID
        /// </summary>
        /// <param name="id">The ID of the order to retrieve</param>
        /// <returns>The requested order information</returns>
        /// <response code="200">Returns the requested order</response>
        /// <response code="404">If the order is not found</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not authorized to view this order</response>
        [HttpGet("GetOrderById/{id}", Name = "GetOrderById")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            var orderResponse = await _orderService.GetOrderByIdAsync(id, userId);
            return ApiResponse<OrderResponseDTO>.OkResult(orderResponse);
        }

        /// <summary>
        /// Retrieves a specific order by its orderID
        /// </summary>
        /// <param name="id">The ID of the order to retrieve</param>
        /// <returns>The requested order information</returns>
        /// <response code="200">Returns the requested order</response>
        /// <response code="404">If the order is not found</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not authorized to view this order</response>

        [HttpGet("GetOrderByOrderId/{id}", Name = "GetOrderByOrderId")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetOrderByorderId(string id)
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            var orderResponse = await _orderService.GetOrderByOrderIdAsync(id, userId);
            return ApiResponse<OrderResponseDTO>.OkResult(orderResponse);
        }

        /// <summary>
        /// Completes an existing order for the authenticated user
        /// </summary>
        /// <param name="orderId">The unique identifier of the order to complete</param>
        /// <response code="200">Order was successfully completed</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="404">Order was not found</response>
        /// <response code="400">Invalid order ID or order cannot be completed</response>
        [HttpPost("CompleteOrder", Name = "CompleteOrder")]
        [Authorize]
        public async Task<IActionResult> CompleteOrder([FromBody] int orderId)
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            await _orderService.CompleteOrderAsync(userId, orderId);
            return ApiResponse.OkResult();
        }

        /// <summary>
        /// Retrieves all orders for a specific user (Admin only)
        /// </summary>
        /// <param name="userId">The ID of the user whose orders to retrieve</param>
        /// <returns>A collection of orders for the specified user</returns>
        /// <response code="200">Returns the list of orders</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not an admin</response>
        [HttpGet("GetUserOrdersAdmin/{userId}", Name = "GetUserOrdersAdmin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserOrdersAdmin(string userId)
        {
            var orderResponse = await _orderService.GetUserOrdersAsync(userId);
            return ApiResponse<IEnumerable<OrderResponseDTO>>.OkResult(orderResponse);
        }

        /// <summary>
        /// Retrieves all orders for a specific user
        /// </summary>
        /// <returns>A collection of orders for the specified user</returns>
        /// <response code="200">Returns the list of orders</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not an admin</response>
        [HttpGet("GetUserOrders", Name = "GetUserOrders")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetUserOrders()
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            var orderResponse = await _orderService.GetUserOrdersAsync(userId);
            return ApiResponse<IEnumerable<OrderResponseDTO>>.OkResult(orderResponse);
        }

        /// <summary>
        /// Updates an existing order for the authenticated user
        /// </summary>
        /// <param name="orderUpdateDTO">The updated order details</param>
        /// <param name="orderId">The ID of the order to update</param>
        /// <returns>The updated order information</returns>
        /// <response code="200">Returns the updated order</response>
        /// <response code="400">If the update details are invalid</response>
        /// <response code="404">If the order is not found</response>
        /// <response code="403">If the user is not authorized to update this order</response>
        [HttpPatch("UpdateUserOrder/{orderId}", Name = "UpdateUserOrder")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateUserOrder([FromBody] OrderUpdateDTO orderUpdateDTO, int orderId)
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            var orderResponse = await _orderService.UpdateOrderAsync(orderUpdateDTO, orderId, userId);
            return ApiResponse<OrderResponseDTO>.OkResult(orderResponse);
        }

        /// <summary>
        /// Updates an order for any user (Admin only)
        /// </summary>
        /// <param name="orderUpdateDTO">The updated order details</param>
        /// <param name="orderId">The ID of the order to update</param>
        /// <param name="userId">The ID of the user whose order is being updated</param>
        /// <returns>The updated order information</returns>
        /// <response code="200">Returns the updated order</response>
        /// <response code="400">If the update details are invalid</response>
        /// <response code="404">If the order is not found</response>
        /// <response code="403">If the user is not an admin</response>
        [HttpPatch("Admin/UpdateUserOrder", Name = "AdminUpdateUserOrder")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserOrder([FromBody] OrderUpdateDTO orderUpdateDTO, [FromQuery] int orderId, [FromQuery] string userId)
        {
            var orderResponse = await _orderService.UpdateOrderAsync(orderUpdateDTO, orderId, userId);
            return ApiResponse<OrderResponseDTO>.OkResult(orderResponse);
        }

        /// <summary>
        /// Deletes an order for the authenticated user
        /// </summary>
        /// <param name="orderId">The ID of the order to delete</param>
        /// <returns>No content on successful deletion</returns>
        /// <response code="204">If the order was successfully deleted</response>
        /// <response code="404">If the order is not found</response>
        /// <response code="403">If the user is not authorized to delete this order</response>
        [HttpDelete("DeleteOrder/{orderId}", Name = "DeleteOrder")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            await _orderService.DeleteOrderAsync(orderId, userId);
            return NoContent();
        }

        /// <summary>
        /// Deletes an order for any user (Admin only)
        /// </summary>
        /// <param name="orderId">The ID of the order to delete</param>
        /// <param name="userId">The ID of the user whose order is being deleted</param>
        /// <returns>No content on successful deletion</returns>
        /// <response code="204">If the order was successfully deleted</response>
        /// <response code="404">If the order is not found</response>
        /// <response code="403">If the user is not an admin</response>
        [HttpDelete("DeleteOrderAdmin/{orderId}", Name = "DeleteOrderAdmin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteOrderAdmin(int orderId, string userId)
        {
            await _orderService.DeleteOrderAsync(orderId, userId);
            return NoContent();
        }
    }
}