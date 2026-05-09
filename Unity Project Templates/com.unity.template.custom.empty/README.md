# Unity 空白项目模板

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Unity](https://img.shields.io/badge/Unity-2020.3+-blue)](https://unity.com/)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-blue)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)

空白项目模板仅提供最少依赖，你可以基于此搭建和设计其它项目模板。

## 依赖项

``` text
"dependencies": {
    "com.unity.ide.visualstudio": "2.0.23",
    "com.unity.ide.vscode": "1.2.5"
  }
```

## 模板目录结构

``` text
（基于 Windows CMD tree 生成）
package
│  package.json
│
└─ProjectData~
    ├─Assets
    │  │  package_blacklist.json
    │  │  TemplateInitializer.cs
    │  │
    │  ├─Plugins
    │  ├─Resources
    │  ├─Scenes
    │  │      Demo.unity
    │  │
    │  ├─Scripts
    │  └─StreamingAssets
    ├─Packages
    └─ProjectSettings
```
