using Microsoft.AspNetCore.Mvc;
using chickko.api.Services;
using chickko.api.Models;
using Microsoft.AspNetCore.Authorization; 
// เพิ่มการใช้งาน Authorize เพื่อให้สามารถตรวจสอบสิทธิ์ได้
using System.Collections.Generic;
namespace chickko.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // เพิ่มการตรวจสอบสิทธิ์
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _service;

        public MenuController(IMenuService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetMenus()
        {
            return Ok(_service.GetMenus());
        }

        [HttpGet("{id}")]
        public IActionResult GetMenu(int id)
        {
            var menu = _service.GetMenu(id);
            if (menu == null) return NotFound();
            return Ok(menu);
        }

        [HttpPost("createmenu")]
        [Authorize] // เพิ่มการตรวจสอบสิทธิ์
        public IActionResult CreateMenu(Menu menu)
        {
            var created = _service.CreateMenu(menu);
            return CreatedAtAction(nameof(GetMenu), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateMenu(int id, Menu menu)
        {
            var updated = _service.UpdateMenu(id, menu);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteMenu(int id)
        {
            var deleted = _service.DeleteMenu(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
        [HttpPost("CopyMenusFromFirestore")]
        public async Task<IActionResult> CopyMenusFromFirestore()
        {
            try
            {
                var result = await _service.CopyMenusFromFirestore();
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}