# unity-quiche-sample

Unity上でQUICプロトコルを試すサンプルです。
クライアント、サーバー両方に対応しています。
QUICプロトコルの実装は[Quiche](https://github.com/cloudflare/quiche)を使用しています。

## 実行方法

**macOS上でしか検証してないことに注意**
[Quiche](https://github.com/cloudflare/quiche)のBuildingを参考にビルドをしてください。
```sh
$ cargo build --release
```
`target/release/libquiche.dylib`を`Assets/Plugins/macOS/`以下にコピー。

Unityエディタから実行をすればQUICが試せます。

## SampleSceneについて

### QuicObject
クライアント側のスクリプトを追加しています。

### QuicServer
サーバ側のスクリプトを追加しています。
