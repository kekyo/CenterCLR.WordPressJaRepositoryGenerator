![WordPress.org](http://s.w.org/style/images/wp-header-logo.png "WordPress.org")

# WordPress日本語サイトの正式版アーカイブを自動的にダウンロードして、Gitリポジトリを構築するツール

## お品書き
* http://ja.wordpress.org/ 固定のURLからスクレイピングしてダウンロードしつつ、ローカルの"WordPress-ja"フォルダ配下にGitリポジトリを生成します。
* 一応、バージョン番号を識別して、古い番号のファイルから新しい番号に向かってコミットを行います。
* また、コミットにバージョン番号のタグを適用します。
* これで生成したリポジトリを、 https://github.com/kekyo/WordPress-ja で公開しています。普通はこっちからforkやcloneした方が早いです。
* Visual Studio 2013 (C#) で書いています。
* 超手抜きコードですが、以下のNuGetパッケージを使っているので、使い方のサンプルにはなるかと思います。
  * CenterCLR.SgmlReader (HTMLスクレイピング)
  * sharpcompress (zip解凍)
  * libgit2sharp (Gitクライアントライブラリ)
  * Microsoft HTTP Library
