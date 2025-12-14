# TextToSpeech (.NET)

Azure Speech を利用して台本を音声化する .NET 8 コンソールアプリです。役名ごとに Azure Neural Voice を割り当て、SSML を生成しながら行単位で音声を合成・ミックスして 1 本の音声トラックを出力します。

## 機能概要
- JSON の台本 (`lineId`, `speaker`, `emotion`, `text`, 任意 `pauseMs`) を読み込み。
- YAML のボイス設定から話者ごとのボイス・話速・ピッチなどを解決。
- Azure Speech Service で行ごとに WAV を合成し、`AudioMixer` で連結。
- Media Foundation (Windows) を利用して MP3 にエクスポート、行ごとの WAV も保持。
- 合成内容を JSON Lines 形式で `logs/` ディレクトリへ記録。

## 準備
- .NET SDK 8.0 以降
- Azure Speech Service リソース (キーとリージョン)
- Windows Media Foundation (Windows では既定で有効、MP3 エクスポートに使用)

### 依存パッケージ
`src/TextToSpeech/TextToSpeech.csproj` では次の NuGet パッケージを使用しています。
- `Microsoft.CognitiveServices.Speech` – Azure Speech SDK
- `YamlDotNet` – `config/voices.yaml` の読み込み
- `DotNetEnv` – `.env` から環境変数を取得
- `NAudio` – WAV の連結と MP3 変換

## 設定
- `.env.sample` を `.env` にコピーし、Azure Speech の資格情報を記入します。
  ```powershell
  Copy-Item .env.sample .env
  # AZURE_SPEECH_KEY / AZURE_SPEECH_REGION を設定
  ```
- `config/voices.yaml`
  - `voices`: 役名→ボイス設定 (style, prosody, file_tag など)
  - `aliases`: ニックネームや別名を正規化するマップ
  - `defaults`: 出力フォーマット、デフォルト休止時間、フォールバック音声
- `data/script_scene1.json`
  - `lineId`, `speaker`, `emotion`, `text`, `pauseMs` を持つ配列

必要に応じて `OUTPUT_AUDIO_FORMAT` 環境変数を設定すると、`--out` で拡張子を省略した場合のエクスポート形式を上書きできます (例: `mp3` / `wav`)。

## ビルド
```powershell
dotnet restore TextToSpeech.sln
dotnet build TextToSpeech.sln
```

## 実行
```powershell
dotnet run --project src/TextToSpeech -- \
  --script data/script_scene1.json \
  --voices config/voices.yaml \
  --out output/scene1/scene1_final.mp3 \
  --log-dir logs
```

### CLI オプション
- `--script` (既定: `data/script_scene1.json`) – 台本 JSON のパス。
- `--voices` (既定: `config/voices.yaml`) – ボイス設定 YAML のパス。
- `--out` (既定: `output/scene1/scene1_final.mp3`) – 最終出力ファイルのパス。
- `--log-dir` (既定: `logs`) – 合成ログ (`synth_<sceneId>.jsonl`) の出力先。
- `--scene-id` – ログや一時ディレクトリ名に利用。省略時は台本ファイル名を使用。

## 出力
- 最終音声: `--out` で指定したパスに MP3 / WAV を生成。
- 行ごとの WAV: `--out` のディレクトリ配下に `<sceneId>/<lineId>_<fileTag>.wav` を保存。
- 合成ログ: `--log-dir` 配下の `synth_<sceneId>.jsonl` に 1 行 1 JSON でメタデータを追記 (ディレクトリは自動生成)。

## プロジェクト構成 (2025-12-05 現在)
```
.
├── TextToSpeech.sln
├── .env.sample
├── README.md
├── config/
│   └── voices.yaml
├── data/
│   └── script_scene1.json
├── output/
│   └── (生成された音声を配置)
└── src/TextToSpeech/
    ├── TextToSpeech.csproj
    ├── CliOptions.cs
    ├── Configuration/
    │   └── VoiceConfiguration.cs
    ├── Models/
    │   ├── DialogueLine.cs
    │   ├── RenderedLine.cs
    │   ├── SynthesisLogEntry.cs
    │   └── VoiceProfile.cs
    └── Services/
        ├── AudioExporter.cs
        ├── AudioMixer.cs
        ├── SpeechSynthesizerService.cs
        ├── SsmlBuilder.cs
        └── WaveConcatenator.cs
```


