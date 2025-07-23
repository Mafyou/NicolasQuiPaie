// Global using statements for NicolasQuiPaieAPI project
// C# 13.0 - Global using directives to reduce boilerplate

// System namespaces
global using System.Text;
global using System.Text.Json;
global using System.Security.Claims;
global using System.ComponentModel.DataAnnotations;

// Microsoft Core namespaces
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.OpenApi.Models;
global using Microsoft.Extensions.DependencyInjection;

// Third-party libraries
global using FluentValidation;
global using AutoMapper;

// Project-specific namespaces
global using NicolasQuiPaieAPI.Infrastructure.Data;
global using NicolasQuiPaieAPI.Infrastructure.Models;
global using NicolasQuiPaieAPI.Infrastructure.Repositories;
global using NicolasQuiPaieAPI.Application.Interfaces;
global using NicolasQuiPaieAPI.Application.Services;
global using NicolasQuiPaieAPI.Application.Validators;
global using NicolasQuiPaieAPI.Application.Mappings;
global using NicolasQuiPaieAPI.Presentation.Endpoints;

// Alias to resolve conflicts
global using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
global using ILoggerGeneric = Microsoft.Extensions.Logging.ILogger;