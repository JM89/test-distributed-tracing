using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shared
{
    public static class ConfigurationExtensions
    {
        public static void LogSettings(this IConfiguration configuration, string sectionName)
        {
            Console.WriteLine(configuration.GetSection(sectionName).FlattenSettings());
        }

        public static string FlattenSettings(this IConfiguration configuration)
        {
            var configs = new List<string>();
            foreach (var settings in configuration.GetChildren())
            {
                var children = settings.GetChildren();
                if (children.Any())
                {
                    configs.Add(settings.FlattenSettings());
                }
                else
                {
                    configs.Add($"{settings.Path}={settings.Value}");
                }
            }
            return string.Join(Environment.NewLine, configs);
        }
    }
}
