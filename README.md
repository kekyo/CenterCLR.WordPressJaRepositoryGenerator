![WordPress.org](http://s.w.org/style/images/wp-header-logo.png "WordPress.org")

# WordPress日本語サイトの正式版アーカイブを自動的にダウンロードして、Gitリポジトリを構築するツール

## お品書き
* http://ja.wordpress.org/ 固定のURLからスクレイピングしてダウンロードしつつ、ローカルの"WordPress-ja"フォルダ配下にGitリポジトリを生成します。
* 一応、バージョン番号を識別して、古い番号のファイルから新しい番号に向かってコミットを行います。
* 次のバージョンをコミットする際には、ワークフォルダのフォルダとファイルを全て削除するので、古いバージョンのゴミファイルが残ったままコミットする事はありません。
* また、コミットにバージョン番号のタグを適用します。
* 取り込み済のバージョンは除外するようになっています。
* ベータ版・RC版は取り込んでいません。
* これで生成したリポジトリを、 https://github.com/kekyo/WordPress-ja で公開しています。普通はこっちからforkやcloneした方が早いです。
* Visual Studio 2013 (C#) で書いています。
* 超手抜きコードですが、以下のNuGetパッケージを使っているので、使い方のサンプルにはなるかと思います。
  * CenterCLR.SgmlReader (HTMLスクレイピング)
  * sharpcompress (zip解凍)
  * libgit2sharp (Gitクライアントライブラリ)
  * Microsoft HTTP Library

## バイナリ
* https://raw.githubusercontent.com/kekyo/CenterCLR.WordPressJaRepositoryGenerator/master/CenterCLR.WordPressJaRepositoryGenerator-1.0.0.0.zip

## 実行方法
* 実行環境として、.NET Framework 4.5.1が必要です。
* コマンドラインで「CenterCLR.WordPressJaRepositoryGenerator.exe」を起動するだけです。WordPress日本語サイトから自動的にスクレイピングを実行し、バイナリと同じフォルダ内の"WordPress-ja"フォルダ配下にコミットします。
* 既に"WordPress-ja"フォルダが存在する場合は、そのGitリポジトリを使用します。存在しない場合は新規に生成します。
* 取り込み済みのバージョンは取り込みません。そのため、新しいバージョンがリリースされたら、単にこのツールを実行すればOKです。

![実行サンプルイメージ](https://raw.githubusercontent.com/kekyo/CenterCLR.WordPressJaRepositoryGenerator/master/ExecutionSample.png "実行サンプルイメージ")
