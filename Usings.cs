// Global using statements for the entire solution
// C# 13.0 - Global usings to reduce repetitive imports across test files

// Core .NET usings
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using System.Net;
global using System.Net.Http;
global using System.Net.Http.Json;
global using System.Text;
global using System.Text.Json;

// Testing framework usings
global using NUnit.Framework;
global using Moq;
global using Shouldly;

// Microsoft Extensions usings
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.EntityFrameworkCore;

// AutoMapper
global using AutoMapper;

// Project-specific usings
global using NicolasQuiPaieData.DTOs;
global using NicolasQuiPaieAPI.Infrastructure.Models;
global using NicolasQuiPaieAPI.Application.Interfaces;
global using NicolasQuiPaieAPI.Application.Services;

// C# 13.0 - Type aliases for better code clarity
global using InfrastructureProposalStatus = NicolasQuiPaieAPI.Infrastructure.Models.ProposalStatus;
global using DtoProposalStatus = NicolasQuiPaieData.DTOs.ProposalStatus;
global using InfrastructureFiscalLevel = NicolasQuiPaieAPI.Infrastructure.Models.FiscalLevel;
global using DtoFiscalLevel = NicolasQuiPaieData.DTOs.FiscalLevel;