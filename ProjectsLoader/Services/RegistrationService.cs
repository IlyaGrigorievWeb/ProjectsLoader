﻿using AutoMapper;
using Contracts.Entities;
using Contracts.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectsLoader.Models.Infos;
using Storages.EntitiesStorage;

namespace ProjectsLoader.Services;

public class RegistrationService
{
    private readonly PostgresContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public RegistrationService(PostgresContext context, IPasswordHasher passwordHasher, IMapper mapper)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }
    
    public async Task<bool> CreateUser(UserInfo userCredentials)
    {
        var user = _mapper.Map<User>(userCredentials);
        
        var passwordHash = _passwordHasher.Hash(user.Password);

        var existingUser = await _context.Users
            .Where(x => x.Id == user.Id)
            .FirstOrDefaultAsync();

        if (existingUser != null)
        {
            throw new InvalidOperationException($"A user with the same GUID already exists");
        }

        var newUser = new User()
        {
            Id = Guid.NewGuid(),
            Login = user.Login,
            Password = passwordHash,
        };

        _context.Users.Add(newUser);

        await _context.SaveChangesAsync();

        return true;
    }
}