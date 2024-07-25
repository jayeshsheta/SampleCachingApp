using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SampleCachingApp.Services;
using System.Collections.Generic;
using System;

namespace SampleCachingApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly ILogger<EmployeeController> _logger;
        private readonly EmployeeService _employeeService;
        private readonly IMemoryCache _cache;

        public EmployeeController(ILogger<EmployeeController> logger, EmployeeService employeeService, IMemoryCache cache)
        {
            _logger = logger;
            _employeeService = employeeService;
            _cache = cache;
        }

        //Sample API Call
        // http://localhost:5067/Employee?filters[Name]=Amit&filters[Designation]=Manager&pageNo=0&pageSize=10&sortProperty=Name&ascendingSort=true
        [HttpGet]
        public IActionResult Get([FromQuery] EmployeeQueryParameters queryParameters)
        {
            List<Employee> cachedEmployees = _cache.Get<List<Employee>>("employee");
            //cachedEmployees = null;
            if (cachedEmployees == null)
            {
                var employees = _employeeService.GetEmployees(
                    queryParameters.Filters,
                    queryParameters.PageNo,
                    queryParameters.PageSize,
                    queryParameters.SortProperty,
                    queryParameters.AscendingSort
                );

                _cache.Set("employee", employees, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                });

                return Ok(employees);
            }

            return Ok(cachedEmployees);
        }
    }
}
