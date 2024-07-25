using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SampleCachingApp.Services
{
    public class EmployeeService
    {
        private readonly SampleCachingAppContext _sampleCachingAppContext;
        private readonly ICacheService _cacheService; // Assuming an ICacheService interface for caching
        private readonly string _codeLogicHash;

        public EmployeeService(SampleCachingAppContext sampleCachingAppContext, ICacheService cacheService)
        {
            _sampleCachingAppContext = sampleCachingAppContext;
            _cacheService = cacheService;
            _codeLogicHash = CalculateCodeLogicHash();
        }

        public List<Employee> GetEmployees(Dictionary<string, string> filters, int pageNo, int pageSize, string sortProperty, bool ascendingSort)
        {
            // Check if code logic has changed
            var storedHash = _cacheService.Get<string>("EmployeeServiceCodeLogicHash");
            if (storedHash == null || storedHash != _codeLogicHash)
            {
                // Invalidate cache if code logic has changed
                _cacheService.Invalidate("EmployeeServiceCacheKey");
                _cacheService.Set("EmployeeServiceCodeLogicHash", _codeLogicHash);
            }

            // Check cache first
            var cacheKey = $"GetEmployees_{string.Join("_", filters.Select(f => f.Key + "=" + f.Value))}_{pageNo}_{pageSize}_{sortProperty}_{ascendingSort}";
            var cachedResult = _cacheService.Get<List<Employee>>(cacheKey);
            if (cachedResult != null)
            {
                return cachedResult;
            }

            // Get employees from database based on provided parameters
            var employeesQueryable = _sampleCachingAppContext.Employee.AsQueryable();

            // Apply filters
            foreach (var filter in filters)
            {
                switch (filter.Key.ToLower())
                {
                    case "name":
                        employeesQueryable = employeesQueryable.Where(x => x.Name.Contains(filter.Value));
                        break;
                    case "designation":
                        employeesQueryable = employeesQueryable.Where(x => x.Designation.Contains(filter.Value));
                        break;
                }
            }

            // Apply sorting
            switch (sortProperty.ToLower())
            {
                case "name":
                    employeesQueryable = ascendingSort ? employeesQueryable.OrderBy(x => x.Name) : employeesQueryable.OrderByDescending(x => x.Name);
                    break;
                case "designation":
                    employeesQueryable = ascendingSort ? employeesQueryable.OrderByDescending(x => x.Designation) : employeesQueryable.OrderByDescending(x => x.Designation);
                    break;
                case "id":
                default:
                    employeesQueryable = ascendingSort ? employeesQueryable.OrderByDescending(x => x.Id) : employeesQueryable.OrderByDescending(x => x.Id);
                    break;
            }

            // Apply paging
            employeesQueryable = employeesQueryable.Skip(pageNo * pageSize).Take(pageSize);

            var employees = employeesQueryable.ToList();
            _cacheService.Set(cacheKey, employees);

            return employees;
        }

        private string CalculateCodeLogicHash()
        {
            // Combine relevant parts of the code into a string
            var codeLogic = @"
            // Apply filters
            foreach (var filter in filters)
            {
                switch (filter.Key.ToLower())
                {
                    case 'name':
                        employeesQueryable = employeesQueryable.Where(x => x.Name.Contains(filter.Value));
                        break;
                    case 'designation':
                        employeesQueryable = employeesQueryable.Where(x => x.Designation.Contains(filter.Value));
                        break;
                }
            }

            // Apply sorting
            switch (sortProperty.ToLower())
            {
                case 'name':
                    employeesQueryable = ascendingSort ? employeesQueryable.OrderBy(x => x.Name) : employeesQueryable.OrderByDescending(x => x.Name);
                    break;
                case 'designation':
                    employeesQueryable = ascendingSort ? employeesQueryable.OrderBy(x => x.Designation) : employeesQueryable.OrderByDescending(x => x.Designation);
                    break;
                case 'id':
                default:
                    employeesQueryable = ascendingSort ? employeesQueryable.OrderBy(x => x.Id) : employeesQueryable.OrderByDescending(x => x.Id);
                    break;
            }

            // Apply paging
            employeesQueryable = employeesQueryable.Skip(pageNo * pageSize).Take(pageSize);
            ";

            // Compute hash
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(codeLogic);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
