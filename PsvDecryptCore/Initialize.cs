﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PsvDecryptCore.Models;
using PsvDecryptCore.Services;

namespace PsvDecryptCore
{
    public class Initialize
    {
        /// <summary>
        ///     Configures the required services and information for course infos.
        /// </summary>
        /// <exception cref="FileNotFoundException">Throws when Psv database cannot be found.</exception>
        /// <exception cref="DirectoryNotFoundException">Throws when Psv courses cannot be found.</exception>
        public static Task<IServiceProvider> ConfigureServicesAsync()
        {
            string envVar = Environment.GetEnvironmentVariable("localappdata");
            string folderPath = string.IsNullOrEmpty(envVar)
                ? Environment.GetEnvironmentVariable("psv")
                : Path.Combine(envVar, "pluralsight");
            string filePath = Path.Combine(folderPath, "pluralsight.db");
            string coursesPath = Path.Combine(folderPath, "courses");
            string output = Path.Combine(AppContext.BaseDirectory, "output");
            if (!Directory.Exists(folderPath) || !File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    $"Psv directory or database cannot be found ({folderPath}) Is PS viewer installed or is \"psv\" environment variable declared?");
            }
            var subDirectories = Directory.GetDirectories(coursesPath);
            if (!Directory.Exists(coursesPath) || !subDirectories.Any())
            {
                throw new DirectoryNotFoundException(
                    "Psv courses not found. Did you not download any courses first?");
            }
            if (!Directory.Exists(output)) Directory.CreateDirectory(output);
            var fileInfo = new PsvInformation
            {
                DirectoryPath = folderPath,
                FilePath = filePath,
                CoursesPath = coursesPath,
                CoursesSubDirectories = subDirectories,
                Output = output
            };
            var collection = new ServiceCollection()
                .AddDbContext<PsvContext>()
                .AddSingleton<LoggingService>()
                .AddSingleton<DecryptionEngine>()
                .AddSingleton<StringProcessor>()
                .AddSingleton(fileInfo)
                .AddLogging();
            var services = collection.BuildServiceProvider();
            return Task.FromResult<IServiceProvider>(services);
        }
    }
}