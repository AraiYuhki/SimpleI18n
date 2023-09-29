# I18n
Joro I18n 翻訳システムパッケージ

プロジェクトに追加する場合は、UPMの+ボタンを押して、Add package from git URLから以下のURLを追加する。
https://github.com/AraiYuhki/SimpleI18n.git?path=Packages/I18n

# 概要
[こちら](https://readouble.com/laravel/9.x/ja/localization.html)の複数形の箇所を元に機能を作成しています。

## 基本機能
キーを元に翻訳テーブルからデータを取得し、現在選択されている言語に変換します。

変数の埋め込みは変数名の前に`:`をつけることで実現できます。

例) 
|||
|:--|:--|
|original|`inline parameter test value is :value`|
|param| `("value", "test")`|
|result|`inline parameter test value is test`|

`複数形` のルールに則って文章を選択する場合は、`|`でそれぞれの文章を区切ってください。

プログラム上で使用する場合は、`I18n.TransChoice`を使用してください。

例)
この記述の場合は、配列のインデックスと同様のルールで文章を選択することが出来ます。

`one|two|three`

|choice|result|
|:--|:--|
|0|`one`|
|1|`two`|
|2|`three`|
|3|`two`|


また、この複数形のルールに選択肢を設定する場合は、以下のルールに則って記述してください。
1. 単一の数値にマッチさせたい場合
  - 数値を`{}`で括ってください。
  - 例) `{0}zero|{1}one`
2. 範囲指定でマッチさせたい場合
  - `[]`で2つの数値をカンマ区切りで 指定してください。
  - 1つ目の数値が下限で、2つ目の数値が上限で、1つ目の数値以上2つ目の数値以下の値の場合にマッチします。
  - 2つ目の数値を`*`にした場合は、下限の数値以上の値の場合にマッチします。
`LocalizeText`を使用する場合は、IsChoicableにチェックを付けて`Choice`に設定してください。

例)
`{0}zero|[1,5]value is between one and five|[6,*]value is greater than six`
|choice|result|
|:--|:--|
|0|`zero`|
|1～5|`value is between one and five`|
|6～|`value is greater than six`|
|-1|`value is between one and five`|

文章は`Choice`に設定された数値を元に選択されます。

いずれにもマッチしない場合は2つ目の選択肢が選ばれたことになります。
