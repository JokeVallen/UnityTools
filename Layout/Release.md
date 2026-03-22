## 1.0.1-beta

> 对 **1.0.0** 版本进行完全重构，目前为公开测试版本，请谨慎投入生产环境。

1. CircleLayout 改名 CircleLayoutGroup。
2. CircleLayoutGroup 对旋转角度限制放宽，改用取模运算，映射任何角度为 [0,360]。
3. CircleLayoutGroup 修复 Reset 时未重新计算布局的问题。
4. AutoLayout 改名 AutoLayoutGroup。
5. AutoLayoutGroup 完全重构，采用更通用、准确和灵活的设计，具体请参考 README 文件。