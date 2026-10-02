using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;

namespace QbKit.New;

public sealed class WorkspaceConfigurator(IOptions<ToolingOptions> options)
{
    public void Configure(string directory, string name)
    {
        var versions = options.Value;
        var packagePath = Path.Combine(directory, "package.json");
        var package = ParseJson(packagePath);
        var scripts = package["scripts"]!.AsObject();
        scripts["test"] = "jest --runInBand";
        scripts["test:watch"] = "jest --watch";
        scripts["lint"] = "ng lint";
        scripts["format"] = "prettier --write .";
        scripts["format:check"] = "prettier --check .";

        var dependencies = package["dependencies"]!.AsObject();
        foreach (var key in dependencies.Select(entry => entry.Key).Where(key => key.StartsWith("@angular/", StringComparison.Ordinal)).ToArray())
            dependencies[key] = versions.Angular;
        dependencies["rxjs"] = "7.8.2";
        dependencies["tslib"] = "2.8.1";

        var devDependencies = package["devDependencies"]!.AsObject();
        foreach (var key in devDependencies.Select(entry => entry.Key).Where(key => key.StartsWith("@angular/", StringComparison.Ordinal)).ToArray())
            devDependencies[key] = versions.Angular;
        devDependencies.Remove("vitest");
        devDependencies["angular-eslint"] = versions.AngularEslint;
        devDependencies["@angular-eslint/builder"] = versions.AngularEslint;
        devDependencies["@eslint/js"] = "9.39.1";
        devDependencies["typescript-eslint"] = "8.71.0";
        devDependencies["eslint"] = "9.39.1";
        devDependencies["eslint-config-prettier"] = "10.1.8";
        devDependencies["jest"] = versions.Jest;
        devDependencies["jest-environment-jsdom"] = versions.Jest;
        devDependencies["jest-preset-angular"] = versions.JestPreset;
        devDependencies["@types/jest"] = "30.0.0";
        devDependencies["jsdom"] = "30.0.0";
        devDependencies["prettier"] = "3.9.9";
        devDependencies["typescript"] = "6.0.3";
        package.Remove("packageManager");
        File.WriteAllText(packagePath, package.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);

        var angularPath = Path.Combine(directory, "angular.json");
        var angular = ParseJson(angularPath);
        var project = angular["projects"]![name]!.AsObject();
        var architect = project["architect"]!.AsObject();
        architect.Remove("test");
        architect["lint"] = new JsonObject
        {
            ["builder"] = "@angular-eslint/builder:lint",
            ["options"] = new JsonObject
            {
                ["lintFilePatterns"] = new JsonArray($"projects/{name}/**/*.ts", $"projects/{name}/**/*.html")
            }
        };
        File.WriteAllText(angularPath, angular.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);

        var specPath = Path.Combine(directory, "projects", name, "tsconfig.spec.json");
        var spec = ParseJson(specPath);
        var compilerOptions = spec["compilerOptions"]!.AsObject();
        compilerOptions["module"] = "CommonJS";
        compilerOptions["types"] = new JsonArray("jest", "node");
        File.WriteAllText(specPath, spec.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);

        File.WriteAllText(Path.Combine(directory, "jest.config.cjs"), """
            const { createCjsPreset } = require('jest-preset-angular/presets');

            module.exports = {
              ...createCjsPreset({ tsconfig: '<rootDir>/projects/{{PROJECT}}/tsconfig.spec.json' }),
              setupFilesAfterEnv: ['<rootDir>/setup-jest.ts'],
              roots: ['<rootDir>/projects'],
            };
            """.Replace("{{PROJECT}}", name) + Environment.NewLine);
        File.WriteAllText(Path.Combine(directory, "setup-jest.ts"), """
            import { setupZonelessTestEnv } from 'jest-preset-angular/setup-env/zoneless';

            setupZonelessTestEnv();
            """ + Environment.NewLine);
        File.WriteAllText(Path.Combine(directory, "eslint.config.js"), """
            const eslint = require('@eslint/js');
            const tseslint = require('typescript-eslint');
            const angular = require('angular-eslint');
            const prettier = require('eslint-config-prettier');

            module.exports = tseslint.config(
              {
                files: ['**/*.ts'],
                extends: [eslint.configs.recommended, ...tseslint.configs.recommended, ...angular.configs.tsRecommended],
                processor: angular.processInlineTemplates,
              },
              {
                files: ['**/*.html'],
                extends: [...angular.configs.templateRecommended, ...angular.configs.templateAccessibility],
              },
              prettier,
            );
            """ + Environment.NewLine);
        File.WriteAllText(Path.Combine(directory, ".prettierignore"), """
            node_modules
            dist
            coverage
            package-lock.json
            .angular
            """ + Environment.NewLine);
    }

    private static JsonObject ParseJson(string path) => JsonNode.Parse(
        File.ReadAllText(path),
        documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true })!.AsObject();
}
