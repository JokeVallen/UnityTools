# Unity 测试项目模板

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Unity](https://img.shields.io/badge/Unity-2020.3+-blue)](https://unity.com/)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-blue)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)

提供单元测试、基准测试和测试覆盖率所需的项目环境。

``` text
"dependencies": {
    "com.unity.ide.visualstudio": "2.0.23",
    "com.unity.ide.vscode": "1.2.5",
    "com.unity.test-framework": "1.1.33",
    "com.unity.test-framework.performance": "3.0.3",
    "com.unity.testtools.codecoverage": "1.2.7"
  }
```

## 模板目录结构

``` text
（基于 Windows CMD tree 生成）
\---package
    |   package.json
    |   
    \---ProjectData~
        +---Assets
        |   |   package_blacklist.json
        |   |   TemplateInitializer.cs
        |   |   Tests.asmdef
        |   |   
        |   +---.Assets
        |   |   \---Common
        |   |           FailedTestsHook.cs
        |   |           
        |   +---EditModeTests
        |   |       EditModeTests.asmdef
        |   |       
        |   +---PlayModeTests
        |   |       PlayModeTests.asmdef
        |   |       
        |   +---Scenes
        |   |       Demo.unity
        |   |       
        |   \---Source
        |           Source.asmdef
        |               
        +---Packages    
        +---ProjectSettings
```

在本文中目录结构主要关注 `ProjectData~/Assets` 目录即可。

`Assets` 目录下包括了几个文件和文件夹，其中 `package_blacklist.json` 是与 `TemplateInitializer.cs` 搭配使用的，后者是一个模板初始化脚本，会在基于模板创建的项目打开时自动执行，主要负责清理 Unity 默认引入的的内置资源包以及将 `.Assets` 目录中的资源重新导入 `Assets` 目录，`package_blacklist.json` 用来定义需要移除的 Unity 默认引入的的内置资源包名单，格式与 `manifest.json` 一样，你可以自主决定保留哪些内置资源包，只需要从黑名单中移除它们即可，黑名单功能本质上是覆写 `manifest.json`。

`.Assets` 目录相当于是模板项目中需要放入 `Assets` 目录的资产副本，因为基于模板创建的项目可能不会主动触发 UPM 的 Resolve，此时 `Packages` 目录为空，任何资源包都未导入，这就导致 `Assets` 目录中若存在需要引用资源包的脚本或其它资产就会引发错误，所以 `TemplateInitializer` 的作用则是主动触发 UPM 的 Resolve 并根据黑名单覆写 `manifest.json`，使得你的 `Packages` 目录下只保留你在设计项目模板时显式要求的资源包，在完成以上行为后它会重新将 `.Assets` 目录中的资源移至 `Assets` 目录并进行刷新。

正常情况下，若项目模板本身不存在错误，例如在 `Assets` 目录而不是 `.Assets` 目录下放置阻断程序集正常编译的资产，那么 `TemplateInitializer` 往往都会正常执行，并且执行完成后会自动销毁自身的脚本文件及`package_blacklist.json`、`.Assets` 等临时资源，你只需要在首次打开基于模板创建的项目时安静等待它完成即可，它本质上是一个自动处理脚本，且会在控制台详细打印处理流程，但阻断它的执行意味着你需要人为介入上述这些繁琐的工作。

记住几个重要提示：
 - 你所有需要放置在 `Assets` 目录中的额外资源应该放在 `.Assets` 目录中。
 - 通过 `package_blacklist.json` 决定需要移除和保留的 Unity 默认引入的内置资源包。
 - 不建议阻断 `TemplateInitializer` 的执行，否则需要人为接续处理流程。

上述目录涵盖了 EditMode 和 PlayMode 进行单元测试的目录，`Source` 是放置待测试源码的目录，`FailedTestsHook.cs` 是一个 Test Runner 出现未通过的测试时自动生成失败测试报告的自动处理脚本，模板已引入 `Unity TestFramework` 、`Unity TestFramework Performance` 和 `Code Coverage` 资源包，分别覆盖了单元测试、基准测试和测试覆盖率的需求。