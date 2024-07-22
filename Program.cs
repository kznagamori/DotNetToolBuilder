using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CsprojEditor
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Usage: CsprojEditor <path_to_csproj>");
				return;
			}

			string csprojPath = args[0];

			if (!File.Exists(csprojPath))
			{
				Console.WriteLine($"Error: The file '{csprojPath}' does not exist.");
				return;
			}

			try
			{
				XDocument doc = XDocument.Load(csprojPath);

				XElement? project = doc.Element("Project");
				if (project == null)
				{
					Console.WriteLine("Error: Invalid csproj file format.");
					return;
				}

				XElement? propertyGroup = project.Elements("PropertyGroup").FirstOrDefault();
				if (propertyGroup == null)
				{
					propertyGroup = new XElement("PropertyGroup");
					project.Add(propertyGroup);
				}

				UpdateOrCreateElement(propertyGroup, "OutputType", "Exe");
				UpdateOrCreateElement(propertyGroup, "PackAsTool", "true");
				UpdateOrCreateElement(propertyGroup, "ToolCommandName", Path.GetFileNameWithoutExtension(csprojPath));
				UpdateOrCreateElement(propertyGroup, "PackageOutputPath", "./nupkg");

				doc.Save(csprojPath);
				Console.WriteLine("csproj file updated successfully.");

				CreateBatchFile(csprojPath, Path.GetFileNameWithoutExtension(csprojPath));
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
			}
		}

		static void UpdateOrCreateElement(XElement parent, string elementName, string value)
		{
			XElement? element = parent.Elements(elementName).FirstOrDefault();
			if (element == null)
			{
				element = new XElement(elementName, value);
				parent.Add(element);
			}
			else
			{
				element.Value = value;
			}
		}

		static void CreateBatchFile(string csprojPath, string baseName)
		{
			string directory = Path.GetDirectoryName(csprojPath) ?? throw new InvalidOperationException("Failed to get directory name.");
			string batchFilePath = Path.Combine(directory, "dotnet_tool_publish.bat");

			string[] lines =
			{
				"@echo off",
				"dotnet pack",
				$"dotnet tool install --global --add-source ./nupkg {baseName}"
			};

			File.WriteAllLines(batchFilePath, lines);
			Console.WriteLine("dotnet_tool_publish.bat file created successfully.");
		}
	}
}
