# SimpleI18n — Lightweight Internationalization System for Unity
## 📘 Overview

SimpleI18n is a lightweight internationalization (i18n) system for Unity, inspired by
Laravel’s Localization (Pluralization)
.

You can easily install it through Unity Package Manager (UPM) via Git URL:

https://github.com/AraiYuhki/SimpleI18n.git?path=Packages/I18n

## 🚀 Installation

Open Window → Package Manager in Unity

Click the + button → Add package from git URL...

Paste the following URL:

https://github.com/AraiYuhki/SimpleI18n.git?path=Packages/I18n

## 🧩 Core Features
### 🔹 Retrieve Translations

Fetch localized text for the current language by specifying a translation key.

I18n.Translate("hello.world");

### 🔹 Variable Embedding

You can dynamically insert variables into text by prefixing them with :.

|Type|Content|
|:--|:--|
|original|inline parameter test value is :value|
|param|("value", "test")|
|result|inline parameter test value is test|

## 🔢Pluralization Support

You can define plural or conditional sentences by separating options with |.

Example 1: Simple pluralization

```C#
I18n.TransChoice("one|two|three", count);
```

|choice|result|
|:--|:--|
|0|one|
|1|two|
|2|three|
|3|two|

Example 2: Range and specific values

Define rules with {} or [] for precise control.

## Syntax	Meaning

{0}	Matches when value = 0

[1,5]	Matches when value is between 1 and 5

[6,*]	Matches when value ≥ 6

`{0}zero|[1,5]value is between one and five|[6,*]value is greater than six`

|choice|result|
|:--|:--|
|0|zero|
|1–5|value is between one and five|
|6+|value is greater than six|
|-1|value is between one and five|
## 🎨 Unity Integration

In the Unity Editor, you can use the LocalizeText component to automatically display translated text.

To enable pluralization, check IsChoicable and assign a value to Choice.

## 📄 License

This project is licensed under the MIT License.

See LICENSE
 for details.

## 💡 Notes

Designed to be simple and lightweight — ideal for small and medium projects.

You can manage translation data in any format (e.g., JSON, ScriptableObject).