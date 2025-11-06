# SimpleI18n — Unity 向け簡易多言語化システム
## 📘 概要

SimpleI18n は Unity プロジェクトに簡単に導入できる軽量な国際化（i18n）システムです。
Laravel の 多言語化機能（複数形対応）
 を参考にして作られています。

Git URL から Unity Package Manager（UPM）経由で導入できます。

https://github.com/AraiYuhki/SimpleI18n.git?path=Packages/I18n

## 🚀 インストール方法

Unity のメニューで Window → Package Manager を開く

+ ボタン → Add package from git URL... を選択

次の URL を入力して追加

https://github.com/AraiYuhki/SimpleI18n.git?path=Packages/I18n

## 🧩 基本機能
### 🔹 翻訳テーブルからの取得

指定された「キー」に基づき、現在選択されている言語に対応する文字列を取得します。

I18n.Translate("hello.world");

### 🔹 変数埋め込み

変数名の前に : を付けることで、文中に動的な値を埋め込むことができます。

種類	内容
```C#
original	inline parameter test value is :value
param	("value", "test")
result	inline parameter test value is test
```
### 🔢 複数形対応（Pluralization）

複数形や条件付きの文を扱いたい場合は、| 区切りで候補を記述します。

例1：単純な複数形
```C#
I18n.TransChoice("one|two|three", count);
```

|choice|結果|
|:--|:--|
|0|one|
|1|two|
|2|three|
|3|two|

例2：範囲・特定数値指定

特殊なルールを {} や [] で指定できます。

記法	意味
{0}	値が0のとき
[1,5]	値が1以上5以下のとき
[6,*]	値が6以上のとき
`{0}zero|[1,5]value is between one and five|[6,*]value is greater than six`

|choice|結果|
|:--|:--|
|0|zero
|1～5|value is between one and five|
|6～|value is greater than six|
|-1|value is between one and five|

## 🎨 Unity Integration

Unity エディタ上では、LocalizeText コンポーネントを使用して自動的に翻訳を行えます。
複数形対応を有効にしたい場合は、IsChoicable にチェックを入れ、Choice に数値を設定します。

## 📄 ライセンス

このプロジェクトは MIT License の下で提供されています。
詳細は LICENSE
 をご確認ください。

## 💡 補足

シンプルかつ軽量に設計されており、小規模プロジェクトでも導入が容易です。

JSONやScriptableObjectなど、任意の形式で翻訳データを管理可能です。