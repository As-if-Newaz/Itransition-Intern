using AutoMapper;
using Iforms.BLL.DTOs;
using Iforms.DAL;
using Iforms.DAL.Entity_Framework.Table_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Iforms.BLL.Services
{
    public class ApiTokenService
    {
        private readonly DataAccess DA;
        public ApiTokenService(DataAccess dataAccess)
        {
            DA = dataAccess;
        }

        static Mapper GetMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ApiToken, ApiTokenDTO>().ReverseMap();
            });
            return new Mapper(config);
        }

        public ApiTokenDTO? GetById(int id)
        {
            var token = DA.ApiTokenData().Get(id);
            return token == null ? null : GetMapper().Map<ApiTokenDTO>(token);
        }

        public IEnumerable<ApiTokenDTO> GetByUserId(int userId)
        {
            var tokens = DA.ApiTokenData().Find(t => t.UserId == userId);
            return GetMapper().Map<List<ApiTokenDTO>>(tokens);
        }

        public ApiTokenDTO? GetByKey(string key)
        {
            var token = DA.ApiTokenData().Find(t => t.Key == key).FirstOrDefault();
            return token == null ? null : GetMapper().Map<ApiTokenDTO>(token);
        }

        public ApiTokenDTO Create(int userId, string? name = null)
        {
            var token = new ApiToken
            {
                Key = GenerateTokenKey(),
                Name = name,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };
            DA.ApiTokenData().Create(token);
            return GetMapper().Map<ApiTokenDTO>(token);
        }

        public bool Revoke(int id)
        {
            var token = DA.ApiTokenData().Get(id);
            if (token == null) return false;
            token.IsRevoked = true;
            return DA.ApiTokenData().Update(token);
        }

        public bool Delete(int id)
        {
            var token = DA.ApiTokenData().Get(id);
            if (token == null) return false;
            return DA.ApiTokenData().Delete(token);
        }

        private string GenerateTokenKey()
        {
            var bytes = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            var base64 = Convert.ToBase64String(bytes);
            var base64Url = base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
            return base64Url;
        }
    }
} 