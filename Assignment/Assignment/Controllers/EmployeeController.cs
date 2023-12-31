﻿using Assignment.DTO;
using Assignment.Models;
using Assignment.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace Assignment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : Controller
    {
        
        private readonly AppDbContext _context;

        public EmployeeController(AppDbContext context)
        {
            _context = context;
        }

        //API01#
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, EmployeeDto updateDto)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);

                if (employee == null)
                {
                    return NotFound();
                }

                // Check for duplicate EmployeeCode
                var existingEmployeeWithCode = await _context.Employees
                    .FirstOrDefaultAsync(e => e.EmployeeCode == updateDto.EmployeeCode && e.EmployeeId != id);

                if (existingEmployeeWithCode != null)
                {
                    return BadRequest("Employee code already exists.");
                }

                employee.EmployeeName = updateDto.EmployeeName;
                employee.EmployeeCode = updateDto.EmployeeCode;
                employee.EmployeeSalary = updateDto.EmployeeSalary;
                employee.SupervisorId = updateDto.SupervisorId;


                _context.Entry(employee).State = EntityState.Modified;


                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle concurrency exception
                return StatusCode(500, "An error occurred while saving the changes.");
            }

            return NoContent();
        }
        
        //API02#
        [HttpGet("thirdhighestsalary")]
        public async Task<ActionResult<EmployeeDto>> GetEmployeeWithThirdHighestSalary()
        {
            try
            {
                var employee = await _context.Employees
                .OrderByDescending(e => e.EmployeeSalary)
                .Skip(2) // Skip the first two highest salaries
                .Take(1) // Take one employee
                .FirstOrDefaultAsync();

                if (employee == null)
                {
                    return NotFound();
                }

                return new EmployeeDto
                {
                    EmployeeId = employee.EmployeeId,
                    EmployeeName = employee.EmployeeName,
                    EmployeeCode = employee.EmployeeCode,
                    EmployeeSalary = employee.EmployeeSalary,
                    SupervisorId = employee.SupervisorId
                };

            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle concurrency exception
                return StatusCode(500, "An error occurred while saving the changes.");
            }   
        }


        //API05#
        [HttpGet("hierarchy/{employeeId}")]
        public IActionResult GetHierarchy(int employeeId)
        {
            try
            {
                var hierarchy = GetHierarchyRecursive(employeeId);
                if (hierarchy == null)
                {
                    return NotFound("Employee not found");
                }

                return Ok(hierarchy);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle concurrency exception
                return StatusCode(500, "An error occurred while saving the changes.");
            }

        }

        private List<string>? GetHierarchyRecursive(int employeeId, HashSet<int>? visitedIds = null)
        {
                if (visitedIds == null)
                {
                    visitedIds = new HashSet<int>();
                }

                if (visitedIds.Contains(employeeId))
                {
                    return new List<string>();
                }

                visitedIds.Add(employeeId);

                var employee = _context.Employees.FirstOrDefault(e => e.EmployeeId == employeeId);
                if (employee == null)
                {
                    return null;
                }

                var hierarchy = new List<string> { employee.EmployeeName };

                if (employee.SupervisorId.HasValue)
                {
                    var supervisorHierarchy = GetHierarchyRecursive(employee.SupervisorId.Value, visitedIds);
                    if (supervisorHierarchy != null)
                    {
                        hierarchy.AddRange(supervisorHierarchy);
                    }
                }

                return hierarchy;      
        }

        /*
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }
        */

        /*
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, EmployeeDto updateDto)
        {
            try
            {
                var updatedEmployee = await _employeeService.UpdateEmployee(id, updateDto);

                if (updatedEmployee == null)
                {
                    return NotFound();
                }

                return Ok(updatedEmployee);
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "An error occurred while saving the changes.");
            }
        }*/
    }

}




