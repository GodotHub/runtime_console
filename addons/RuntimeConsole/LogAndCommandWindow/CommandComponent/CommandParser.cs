using System.Collections.Generic;
using System.Linq;
using Godot;
using static RuntimeConsole.CheckInterfaceUtil;

namespace RuntimeConsole;

public class CommandParser
{
	readonly Dictionary<Variant.Type, IParameterParser> _parsers;
	readonly List<IConsoleCommand> _commands;

	readonly List<GDScriptMethodInfo> _iparameterParserInterface = [
		GDScriptMethodInfo.Getter(
			propertyName: "supported_types",
			propertyClassName: "Variant.Type",
			propertyType: Variant.Type.Array,
			propertyKind: GDScriptMethodInfo.PropertyKind.TypedArray
		),
		GDScriptMethodInfo.Getter(
			propertyName: "result",
			propertyClassName: "",
			propertyType: Variant.Type.Nil,
			propertyKind: GDScriptMethodInfo.PropertyKind.Variant
		),
		GDScriptMethodInfo.EnumMethod(
			name: "parse",
			argsCount: 1,
			argsClassName: [""],
			argsType: [Variant.Type.String],
			enumName: "Error"
		)
	];

	readonly List<GDScriptMethodInfo> _icommandInterface = [
		GDScriptMethodInfo.Getter(
			propertyName: "keyword",
			propertyClassName: "",
			propertyType: Variant.Type.String
		),
		GDScriptMethodInfo.Getter(
			propertyName: "parameter_types",
			propertyClassName: "",
			propertyType: Variant.Type.Array,
			propertyKind: GDScriptMethodInfo.PropertyKind.TypedArray,
			arrayType: "Variant.Type"
		),
		GDScriptMethodInfo.VoidMethod(
			name: "execute",
			argsCount: 1,
			argsClassName: [""],
			argsType: [Variant.Type.Array]
		)
	];

	public CommandParser(string parsersPath, string commandsPath)
	{
		var parserFiles = ResourceLoader.ListDirectory(parsersPath);
		_parsers = [];
		foreach (var file in parserFiles)
		{
			var script = ResourceLoader.Load<Script>(parsersPath.PathJoin(file));
			if (script is null || !script.CanInstantiate())
				continue;

			IParameterParser parser = null;

			if (script is GDScript gdScript)
			{
				if (!IsImplementedInterface(gdScript, _iparameterParserInterface))
				{
					GD.PushError($"Script {file} does not implement IParameterParser interface.");
					continue;
				}
				parser = new GDScriptParserWrapper(gdScript);
			}
			else if (script is CSharpScript csharpScript)
			{
				if (csharpScript.New().Obj is not IParameterParser instance)
				{
					GD.PushError($"Script {file} does not implement IParameterParser interface.");
					continue;
				}

				parser = instance;
			}
			else
			{
				GD.PushError($"Script {file} is not a valid GDScript or C# script.");
			}

			if (parser != null)
			{
				foreach (var type in parser.SupportedTypes)
				{
					if (_parsers.ContainsKey(type))
					{
						GD.PushError($"Repeat parser registration: A parser already exists for Type {type}!");
						continue;
					}

					_parsers[type] = parser;
					// GD.Print($"Registered parser for {type} => {file}");
				}
			}
		}

		var commandFiles = ResourceLoader.ListDirectory(commandsPath);
		_commands = [];
		foreach (var file in commandFiles)
		{
			var script = ResourceLoader.Load<Script>(commandsPath.PathJoin(file));
			if (script is null || !script.CanInstantiate())
			{
				continue;
			}

			if (script is GDScript gdScript)
			{
				if (!IsImplementedInterface(gdScript, _icommandInterface))
				{
					GD.PushError($"Script {file} does not implement IConsoleCommand interface.");
					continue;
				}
				_commands.Add(new GDScriptCommandWrapper(gdScript));
			}
			else if (script is CSharpScript csharpScript)
			{
				if (csharpScript.New().Obj is not IConsoleCommand instance)
				{
					GD.PushError($"Script {file} does not implement IConsoleCommand interface.");
					continue;
				}
				_commands.Add(instance);
			}
			else
			{
				GD.PushError($"Script {file} is not a valid GDScript or C# script.");				
			}
		}

	}

	public enum ParseResult
	{
		OK,
		NoInput,
		ArgParseFailed,
		ArgsNotEnough,
		CommandNotFound
	}

	/// <summary>
	/// 获取错误信息
	/// </summary>
	/// <param name="result">错误信息枚举</param>
	/// <returns>错误信息枚举的详细信息</returns>
	public static string GetParseMessage(ParseResult result)
		=> result switch
		{
			ParseResult.NoInput => "No input received. Please enter a command.",
			ParseResult.CommandNotFound => "Command not found.",
			ParseResult.ArgsNotEnough => "Missing arguments. This command requires more parameters.",
			ParseResult.ArgParseFailed => "Failed to parse one or more arguments. Make sure the values match the expected types.",
			_ => "Unknown error occurred while parsing the command."
		};

	/// <summary>
	/// 解析给定的输入命令
	/// </summary>
	/// <param name="input">命令原始字符串输入</param>
	/// <returns>解析结果枚举，成功时返回OK，使用<see cref="GetParseMessage(ParseResult)"/>获取详细信息</returns>
	public ParseResult Parse(string input)
	{
		var tokens = Tokenizer(input);
		if (tokens.Length == 0)
			return ParseResult.NoInput;

		var keyword = tokens[0];
		var rawArgs = tokens.Skip(1).ToArray();

		foreach (var command in _commands)
		{

			if (command.Keyword != keyword)
				continue;

			var expectedTypes = command.ParameterTypes;

			if (expectedTypes.Length != rawArgs.Length)
			{
				// GD.PushError($"Mismatched number of arguments: {expectedTypes.Length} is required, and {rawArgs.Length} is received");
				return ParseResult.ArgsNotEnough;
			}
			var parsedArgs = new List<Variant>();

			for (int i = 0; i < rawArgs.Length; i++)
			{
				var parsed = TryParseArgumentByType(rawArgs[i], expectedTypes[i]);
				if (!parsed.HasValue)
				{
					// GD.PushError($"Argument parsing failed: the {i + 1} argument '{rawArgs[i]}' cannot be converted to {expectedTypes[i]}");
					return ParseResult.ArgParseFailed;
				}
				parsedArgs.Add(parsed.Value);
			}

			command.Execute(new(parsedArgs));
			return ParseResult.OK;
		}

		// GD.PushError($"Command not found: {keyword}");
		return ParseResult.CommandNotFound;
	}

	private Variant? TryParseArgumentByType(string input, Variant.Type expectedType)
	{

		if (_parsers.TryGetValue(expectedType, out var parser))
		{
			if (parser.Parse(input) == Error.Ok)
				return parser.Result;
		}
		else
		{
			GD.PushError($"Could not find parser for {expectedType}");
		}
		return null;
	}

	private string[] Tokenizer(string command)
	{
		List<string> tokens = [];
		var firstSpaceIdx = command.IndexOf(' ');
		if (firstSpaceIdx == -1)
		{
			return [command];
		}

		var keyword = command[..firstSpaceIdx];
		tokens.Add(keyword);
		var args = command[(firstSpaceIdx + 1)..];

		var jsonDepth = 0;
		var inJson = false;
		var tokenStartIdx = 0;
		for (int i = 0; i < args.Length; i++)
		{
			char c = args[i];

			// json结构起始增加深度
			if ((c == '{' || c == '[') && !inJson)
			{
				inJson = true;
				jsonDepth = 1;
				tokenStartIdx = i;
			}
			else if (c == '{' || c == '[')
			{
				jsonDepth++;
			}
			// json结构闭合减少深度
			else if (c == '}' || c == ']')
			{
				jsonDepth--;
				if (jsonDepth == 0 && inJson)
				{
					// 提取json
					var json = args.Substring(tokenStartIdx, i - tokenStartIdx + 1);
					tokens.Add(json);
					inJson = false;
					tokenStartIdx = i + 1;
				}
			}
			else if (c == ' ' && !inJson)
			{
				if (i > tokenStartIdx)
				{
					var token = args[tokenStartIdx..i];
					tokens.Add(token);
				}
				tokenStartIdx = i + 1;
			}

		}

		// 最后一个token
		if (tokenStartIdx < args.Length)
		{
			var last = args[tokenStartIdx..];
			if (!string.IsNullOrWhiteSpace(last))
				tokens.Add(last);
		}

		return tokens.ToArray();
	}

	
}