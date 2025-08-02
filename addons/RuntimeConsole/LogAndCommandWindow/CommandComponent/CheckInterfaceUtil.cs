using System.Collections.Generic;
using System.Linq;
using Godot;

namespace RuntimeConsole;

public class CheckInterfaceUtil
{
    /// <summary>
	/// GDScript方法信息
	/// </summary>
	/// <param name="Name">方法名称</param>
	/// <param name="ArgsCount">参数数量</param>
	/// <param name="ArgsClassName">参数类名列表，长度应与参数数量相匹配，非Object类型填入空字符串</param>
	/// <param name="ArgsType">参数类型列表，长度应与参数数量相匹配</param>
	/// <param name="ReturnClassName">返回值类名，返回非Object时，填入空字符串</param>
	/// <param name="ReturnType">返回值类型</param>
	/// <param name="Aliases">
	/// 方法别名列表（用于getter/setter的检查）<br/>
	/// "@属性名_getter" 和 "@属性名_setter" 用于检查属性的 getter 和 setter 方法。<br/>
	/// 如果不需要别名，可以传入 null 或空数组。
	/// </param>
	/// <param name="ReturnFlags">
	/// 返回值属性用法标记，默认值 -1 表示不检查该属性。<br/>
	/// <see cref="PropertyUsageFlags.ClassIsEnum"/>" 表示返回值必须是一个枚举。<br/>
	/// 此时属性<paramref name="ReturnClassName"/> 应为枚举全名如 “MyClass.MyEnum”/“脚本路径.MyEnum”<br/>
	/// 属性<paramref name="ReturnType"/>应为<see cref="Variant.Type.Int"/>
	/// <see cref="PropertyUsageFlags.NilIsVariant"/> 表示返回值必须为Variant。<br/>
	/// 此时属性<paramref name="ReturnClassName"/> 应为空字符串<br/>
	/// 属性<paramref name="ReturnType"/>应为<see cref="Variant.Type.Nil"/> <br/>
	/// void 方法的返回值属性用法标记大多时候为<see cref="PropertyUsageFlags.Default"/>有时也会是<see cref="PropertyUsageFlags.None">。<br/>
	/// </param>
	/// <param name="ReturnHint">
	/// 返回值提示, 默认值 -1 表示不检查该属性。<br/>
	/// 当属性为<see cref="PropertyHint.ArrayType"/> 时，指示返回值必须为类型化数组，对应的类型在<paramref name="ReturnHintString"/>中。<br/>
	/// 当属性为<see cref="PropertyHint.DictionaryType"/> 时，指示返回值必须为类型化字典，对应类型在<paramref name="ReturnHintString"/>中。<br/>
	/// </param>
	/// <param name="ReturnHintString">
	/// 返回值提示字符串，默认值为空字符串表示不检查该属性。<br/>
	/// 当<paramref name="ReturnHint"/>为<see cref="PropertyHint.ArrayType"/>时，表示返回值必须为类型化数组，该属性为元素类型名。如"int" (返回类型：Array[int])<br/>
	/// 当<paramref name="ReturnHint"/>为<see cref="PropertyHint.DictionaryType"/>时，表示返回值必须为类型化字典，该属性为键值对类型名，如"String;int" (返回类型：Dictionary[String, int])
	/// </param>
	/// <param name="ArgsFlags">
	/// 参数列表的属性用法标记，默认值 null 表示不检查该属性，元素为 -1 表示不检查该参数。<br/>
	/// 索引必须和需要检查的参数列表一致。<br/>
	/// 具体含义参考<paramref name="ReturnFlags"/>		
	/// </param>  	
	/// <param name="ArgsHints">
	/// 参数列表的属性提示，默认值 null 表示不检查该属性，元素为 -1 表示不检查该参数。<br/>
	/// 索引必须和需要检查的参数列表一致。<br/>
	/// 具体含义参考<paramref name="ReturnHint"/>
	/// </param>
	/// <param name="ArgsHintsStrings">
	/// 参数列表的属性提示字符串，默认值 null 表示不检查该属性，元素为 null 表示不检查该参数。<br/>
	/// 索引必须和需要检查的参数列表一致。<br/>
	/// 具体含义参考<paramref name="ReturnHintString"/>
	/// </param>
	public record class GDScriptMethodInfo(
		string Name,
		int ArgsCount,
		string[] ArgsClassName,
		Variant.Type[] ArgsType,
		string ReturnClassName,
		Variant.Type ReturnType,
		string[] Aliases = null,
		PropertyUsageFlags ReturnFlags = (PropertyUsageFlags)(-1),
		PropertyHint ReturnHint = (PropertyHint)(-1),
		string ReturnHintString = "",
		PropertyUsageFlags[] ArgsFlags = null,
		PropertyHint[] ArgsHints = null,
		string[] ArgsHintsStrings = null
	)
	{
		/// <summary>
		/// 构造一个void方法信息 <br/>
		/// 详细参数说明见<see cref="GDScriptMethodInfo(string, int, string[], Variant.Type[], string, Variant.Type, string[], PropertyUsageFlags, PropertyHint, string, PropertyUsageFlags[], PropertyHint[], string[])"/> 
		/// </summary>
		/// <param name="name">方法的名称</param>
		/// <param name="argsCount">参数数量</param>
		/// <param name="argsClassName">参数类型名列表，非Object类型填入空字符串</param>
		/// <param name="argsType">参数类型列表</param>
		/// <param name="aliases">方法的别名</param>
		/// <param name="argsFlags">参数列表的属性用法标记，使用默认值不检查该属性</param>
		/// <param name="argsHints">参数列表的属性提示，使用默认值不检查该属性</param>
		/// <param name="argsHintsStrings">参数列表的属性提示字符串，使用默认值不检查该属性</param>
		/// <returns>一个代表void方法的GDScript方法信息实例</returns>
		public static GDScriptMethodInfo VoidMethod(
			string name,
			int argsCount,
			string[] argsClassName,
			Variant.Type[] argsType,
			string[] aliases = null,
			PropertyUsageFlags[] argsFlags = null,
			PropertyHint[] argsHints = null,
			string[] argsHintsStrings = null
		) => new(
				name,
				argsCount,
				argsClassName,
				argsType,
				string.Empty,
				Variant.Type.Nil,
				aliases,
				PropertyUsageFlags.Default,
				(PropertyHint)(-1),
				string.Empty,
				argsFlags,
				argsHints,
				argsHintsStrings
			);

		/// <summary>
		/// 构造一个返回值为 Variant 的方法信息 <br/>
		/// 详细参数说明见<see cref="GDScriptMethodInfo(string, int, string[], Variant.Type[], string, Variant.Type, string[], PropertyUsageFlags, PropertyHint, string, PropertyUsageFlags[], PropertyHint[], string[])"/> 
		/// </summary>
		/// <param name="name">方法的名称</param>
		/// <param name="argsCount">参数数量</param>
		/// <param name="argsClassName">参数类型名列表，非Object类型填入空字符串</param>
		/// <param name="argsType">参数类型列表</param>
		/// <param name="aliases">方法的别名</param>
		/// <param name="argsFlags">参数列表的属性用法标记，使用默认值不检查该属性</param>
		/// <param name="argsHints">参数列表的属性提示，使用默认值不检查该属性</param>
		/// <param name="argsHintsStrings">参数列表的属性提示字符串，使用默认值不检查该属性</param>
		/// <returns>一个代表返回值为Variant类型方法的GDScript方法信息实例</returns>
		public static GDScriptMethodInfo VariantMethod(
			string name,
			int argsCount,
			string[] argsClassName,
			Variant.Type[] argsType,
			string[] aliases = null,
			PropertyUsageFlags[] argsFlags = null,
			PropertyHint[] argsHints = null,
			string[] argsHintsStrings = null
		) => new(
				name,
				argsCount,
				argsClassName,
				argsType,
				string.Empty,
				Variant.Type.Nil,
				aliases,
				PropertyUsageFlags.NilIsVariant,
				(PropertyHint)(-1),
				string.Empty,
				argsFlags,
				argsHints,
				argsHintsStrings
			);

		/// <summary>
		/// 构造一个无参void方法信息
		/// </summary>
		/// <param name="name">方法的名称</param>
		/// <param name="aliases">方法的别名</param>
		/// <returns>一个代表无参void方法的GDScript方法信息实例</returns>
		public static GDScriptMethodInfo ParameterlessVoidMethod(
			string name,
			string[] aliases = null
		) => VoidMethod(name, 0, [], [], aliases);

		/// <summary>
		/// 构造一个返回值为枚举类型的方法信息 <br/>
		/// 详细参数说明见<see cref="GDScriptMethodInfo(string, int, string[], Variant.Type[], string, Variant.Type, string[], PropertyUsageFlags, PropertyHint, string, PropertyUsageFlags[], PropertyHint[], string[])"/> 
		/// </summary>
		/// <param name="name">方法的名称</param>
		/// <param name="argsCount">参数数量</param>
		/// <param name="argsClassName">参数类型名列表，非Object类型填入空字符串</param>
		/// <param name="argsType">参数类型列表</param>
		/// <param name="enumName">方法返回的枚举类型名，如"MyClass.MyEnum", "res://script.gd.MyEnum","脚本路径.MyEnum"</param>
		/// <param name="aliases">方法的别名</param>
		/// <param name="argsFlags">参数列表的属性用法标记，使用默认值不检查该属性</param>
		/// <param name="argsHints">参数列表的属性提示，使用默认值不检查该属性</param>
		/// <param name="argsHintsStrings">参数列表的属性提示字符串，使用默认值不检查该属性</param>
		/// <returns>一个代表返回枚举类型的方法的GDScript方法信息实例</returns>
		public static GDScriptMethodInfo EnumMethod(
			string name,
			int argsCount,
			string[] argsClassName,
			Variant.Type[] argsType,
			string enumName,
			string[] aliases = null,
			PropertyUsageFlags[] argsFlags = null,
			PropertyHint[] argsHints = null,
			string[] argsHintsStrings = null
		) => new(
				name,
				argsCount,
				argsClassName,
				argsType,
				enumName,
				Variant.Type.Int,
				aliases,
				PropertyUsageFlags.ClassIsEnum,
				(PropertyHint)(-1),
				string.Empty,
				argsFlags,
				argsHints,
				argsHintsStrings
			);

		/// <summary>
		/// 构造一个返回类型化数组的方法信息 <br/>
		/// 详细参数说明见<see cref="GDScriptMethodInfo(string, int, string[], Variant.Type[], string, Variant.Type, string[], PropertyUsageFlags, PropertyHint, string, PropertyUsageFlags[], PropertyHint[], string[])"/> 
		/// </summary>
		/// <param name="name">方法的名称</param>
		/// <param name="argsCount">参数数量</param>
		/// <param name="argsClassName">参数类型名列表，非Object类型填入空字符串</param>
		/// <param name="argsType">参数类型列表</param>
		/// <param name="arrayType">数组的元素类型名，如"int", "Node"</param>
		/// <param name="aliases">方法的别名</param>
		/// <param name="argsFlags">参数列表的属性用法标记，使用默认值不检查该属性</param>
		/// <param name="argsHints">参数列表的属性提示，使用默认值不检查该属性</param>
		/// <param name="argsHintsStrings">参数列表的属性提示字符串，使用默认值不检查该属性</param>
		/// <returns>一个代表返回类型化数组类型的方法的GDScript方法信息实例</returns>
		public static GDScriptMethodInfo TypedArrayMethod(
			string name,
			int argsCount,
			string[] argsClassName,
			Variant.Type[] argsType,
			string arrayType,
			string[] aliases = null,
			PropertyUsageFlags[] argsFlags = null,
			PropertyHint[] argsHints = null,
			string[] argsHintsStrings = null
		) => new(
				name,
				argsCount,
				argsClassName,
				argsType,
				string.Empty,
				Variant.Type.Array,
				aliases,
				(PropertyUsageFlags)(-1),
				PropertyHint.ArrayType,
				arrayType,
				argsFlags,
				argsHints,
				argsHintsStrings
			);

		/// <summary>
		/// 构造一个返回类型化字典的方法信息 <br/>
		/// 详细参数说明见<see cref="GDScriptMethodInfo(string, int, string[], Variant.Type[], string, Variant.Type, string[], PropertyUsageFlags, PropertyHint, string, PropertyUsageFlags[], PropertyHint[], string[])"/> 
		/// </summary>
		/// <param name="name">方法的名称</param>
		/// <param name="argsCount">参数数量</param>
		/// <param name="argsClassName">参数类型名列表，非Object类型填入空字符串</param>
		/// <param name="argsType">参数类型列表</param>
		/// <param name="keyType">字典键的类型名，如"String","Variant"</param>
		/// <param name="valueType">字典值的类型名，如"int", "flaot"</param>
		/// <param name="aliases">方法的别名</param>
		/// <param name="argsFlags">参数列表的属性用法标记，使用默认值不检查该属性</param>
		/// <param name="argsHints">参数列表的属性提示，使用默认值不检查该属性</param>
		/// <param name="argsHintsStrings">参数列表的属性提示字符串，使用默认值不检查该属性</param>
		/// <returns>一个代表返回类型化字典类型的方法的GDScript方法信息实例</returns>
		public static GDScriptMethodInfo TypedDictionaryMethod(
			string name,
			int argsCount,
			string[] argsClassName,
			Variant.Type[] argsType,
			string keyType,
			string valueType,
			string[] aliases = null,
			PropertyUsageFlags[] argsFlags = null,
			PropertyHint[] argsHints = null,
			string[] argsHintsStrings = null
		) => new(
				name,
				argsCount,
				argsClassName,
				argsType,
				string.Empty,
				Variant.Type.Dictionary,
				aliases,
				(PropertyUsageFlags)(-1),
				PropertyHint.DictionaryType,
				$"{keyType};{valueType}",
				argsFlags,
				argsHints,
				argsHintsStrings
			);

		public enum PropertyKind
		{
			Normal,           // 普通类型
			Enum,             // 枚举
			Variant,          // Variant 类型
			TypedArray,       // 类型化数组
			TypedDictionary   // 类型化字典
		}

		/// <summary>
		/// 构造一个指定属性getter访问器方法信息<br/>
		/// 具名getter遵循命名规范：get_属性名称
		/// </summary>
		/// <param name="propertyName">属性的名称</param>
		/// <param name="propertyClassName">属性的类名，非Object类型填入空字符串，属性为枚举时填入枚举类型名</param>
        /// <param name="propertyType">属性的类型，Variant类型请将参数<paramref name="propertyKind"/>指定为<see cref="PropertyKind.Variant"/></param>
		/// <param name="propertyKind">属性的类型，默认为普通类型</param>
		/// <param name="arrayType">该属性的类型化数组元素类型</param>
		/// <param name="keyType">该属性的类型化字典的键类型</param>
		/// <param name="valueType">该属性的类型化字典的值类型</param>
		/// <returns>一个代表属性getter的GDScript方法信息</returns>
		public static GDScriptMethodInfo Getter(
			string propertyName,
			string propertyClassName,
			Variant.Type propertyType,
			PropertyKind propertyKind = PropertyKind.Normal,
			string arrayType = "",
			string keyType = "",
			string valueType = ""
		) => propertyKind switch
            {
				PropertyKind.TypedArray
					=> TypedArrayMethod(
						$"get_{propertyName}",
						0,
						[],
						[],
						arrayType,
						[$"@{propertyName}_getter"]
					),
				PropertyKind.TypedDictionary
					=> TypedDictionaryMethod(
						$"get_{propertyName}",
						0,
						[],
						[],
						keyType,
						valueType,
						[$"@{propertyName}_getter"]
					),
				PropertyKind.Enum
					=> EnumMethod(
						$"get_{propertyName}",
						0,
						[],
						[],
						propertyClassName,
						[$"@{propertyName}_getter"]
					),
				PropertyKind.Variant
					=> VariantMethod(
						$"get_{propertyName}",
						0,
						[],
						[],
						[$"@{propertyName}_getter"]
					),
                _ => new(
						$"get_{propertyName}",
						0,
						[],
						[],
						propertyClassName,
						propertyType,
						[$"@{propertyName}_getter"]
					),
            };


        /// <summary>
        /// 构造一个指定属性setter访问器方法信息<br/>
        /// 具名setter遵循命名规范：set_属性名称
        /// </summary>
        /// <param name="propertyName">属性的名称</param>
        /// <param name="propertyClassName">属性的类名，非Object类型填入空字符串，属性为枚举时填入枚举类型名</param>
        /// <param name="propertyType">属性的类型，Variant类型请将参数<paramref name="propertyKind"/>指定为<see cref="PropertyKind.Variant"/></param>
		/// <param name="propertyKind">属性的类型，默认为普通类型</param>
        /// <param name="arrayType">该属性的类型化数组元素类型</param>
        /// <param name="keyType">该属性的类型化字典的键类型</param>
        /// <param name="valueType">该属性的类型化字典的值类型</param>
        /// <returns>一个代表属性setter的GDScript方法信息</returns>
        public static GDScriptMethodInfo Setter(
            string propertyName,
            string propertyClassName,
            Variant.Type propertyType,
            PropertyKind propertyKind = PropertyKind.Normal,
            string arrayType = "",
            string keyType = "",
            string valueType = ""
        ) => VoidMethod(
                $"set_{propertyName}",
                1,
                [propertyClassName],
                [propertyType],
                [$"@{propertyName}_setter"],
                propertyKind == PropertyKind.Variant ? [PropertyUsageFlags.NilIsVariant] : propertyKind == PropertyKind.Enum ? [PropertyUsageFlags.ClassIsEnum] : [],
                propertyKind == PropertyKind.TypedArray ? [PropertyHint.ArrayType] : propertyKind == PropertyKind.TypedDictionary ? [PropertyHint.DictionaryType] : [],
                propertyKind == PropertyKind.TypedArray ? [arrayType] : propertyKind == PropertyKind.TypedDictionary ? [$"{keyType};{valueType}"] : []
            );
    }

    /// <summary>
    /// 检查给定 GDScript 是否实现了指定接口（包含所需方法）。
    /// </summary>
    /// <param name="script">待检查的 GDScript 实例</param>	
    /// <param name="interface">GDScript 所需方法签名列表</param>
    /// <returns>如果脚本实现了接口（包含所有所需方法），返回 true，否则返回 false。</returns>
    public static bool IsImplementedInterface(GDScript script, List<GDScriptMethodInfo> @interface)
    {
        var methodInfo = script.GetScriptMethodList();

        return @interface.All(req => methodInfo.Any(m =>
            {
                var name = m["name"].AsString();
                var args = m["args"].AsGodotArray<Godot.Collections.Dictionary>();
                var ret = m["return"].AsGodotDictionary();

                // 检查方法名称是否匹配
                var validNames = req.Aliases?.Append(req.Name) ?? [req.Name];
                if (!validNames.Contains(name))
                    return false;

                // 检查参数数量是否匹配
                if (args.Count != req.ArgsCount)
                    return false;

                // 检查参数类型和类名是否匹配
                for (int i = 0; i < args.Count; i++)
                {
                    if (args[i]["type"].As<Variant.Type>() != req.ArgsType[i] &&
                        args[i]["class_name"].AsStringName() != req.ArgsClassName[i])
                        return false;

                    if (req.ArgsFlags?.Length > i && req.ArgsFlags[i] != (PropertyUsageFlags)(-1))
                        if ((args[i]["usage"].As<PropertyUsageFlags>() & req.ArgsFlags[i]) != req.ArgsFlags[i])
                            return false;

                    if (req.ArgsHints?.Length > i && req.ArgsHints[i] != (PropertyHint)(-1))
                        if (args[i]["hint"].As<PropertyHint>() != req.ArgsHints[i])
                            return false;

                    if (req.ArgsHintsStrings?.Length > i && !string.IsNullOrEmpty(req.ArgsHintsStrings[i]))
                        if (args[i]["hint_string"].AsString() != req.ArgsHintsStrings[i])
                            return false;
                }

                // 检查返回值类型和类名是否匹配
                if (ret["type"].As<Variant.Type>() != req.ReturnType &&
                    ret["class_name"].AsStringName() != req.ReturnClassName)
                    return false;

                if (req.ReturnFlags != (PropertyUsageFlags)(-1) && (ret["usage"].As<PropertyUsageFlags>() & req.ReturnFlags) != req.ReturnFlags)
                    return false;

                if (req.ReturnHint != (PropertyHint)(-1) && ret["hint"].As<PropertyHint>() != req.ReturnHint)
                    return false;

                if (!string.IsNullOrEmpty(req.ReturnHintString) && ret["hint_string"].AsString() != req.ReturnHintString)
                    return false;

                return true;
            }));

    }
}