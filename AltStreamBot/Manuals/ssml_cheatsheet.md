# SSML Cheat Sheet

## 1. Basic Structure
```xml
<speak version="1.0" xmlns="http://www.w3.org/2001/10/synthesis" xml:lang="en-US">
  <voice name="Voice Name">
    Text or other SSML tags here
  </voice>
</speak>
```
- `version` is always 1.0.
- `xmlns` is required for SSML.
- `xml:lang` sets language.
- `voice name` must match an installed Windows TTS voice.

## 2. Voice
```xml
<voice name="Microsoft Zira Desktop">Hello, I am Zira!</voice>
```
- Use `synth.GetInstalledVoices()` to see available voices.
- Can switch voices mid-sentence.

## 3. Prosody (Pitch, Rate, Volume)
```xml
<prosody rate="+20%" pitch="+10%" volume="loud">
  I am speaking faster, higher, and louder!
</prosody>
```
- `rate` Å® speed: `x-slow`, `slow`, `medium`, `fast`, `x-fast` or percentages (+20%, -10%).
- `pitch` Å® voice pitch: `x-low`, `low`, `medium`, `high`, `x-high` or +/- percentages.
- `volume` Å® loudness: `silent`, `x-soft`, `soft`, `medium`, `loud`, `x-loud` or +/-dB.

## 4. Emphasis
```xml
<emphasis level="strong">Pay attention to this!</emphasis>
```
- `level`: `strong`, `moderate`, `reduced`.

## 5. Breaks (Pauses)
```xml
<break time="500ms"/>
<break strength="medium"/>
```
- `time` in milliseconds or seconds (2s).
- `strength`: `none`, `x-weak`, `weak`, `medium`, `strong`, `x-strong`.

## 6. Say-As (Numbers, Dates, etc.)
```xml
<say-as interpret-as="digits">12345</say-as>
<say-as interpret-as="date">2025-12-07</say-as>
<say-as interpret-as="time">14:30</say-as>
```
- `interpret-as` can be `characters`, `digits`, `number`, `date`, `time`, `telephone`, `spell-out`.

## 7. Combining Tags
```xml
<speak version="1.0" xmlns="http://www.w3.org/2001/10/synthesis" xml:lang="en-US">
  <voice name="Microsoft David Desktop">
    <prosody rate="-10%" pitch="+5%">Hello!</prosody>
    <break time="300ms"/>
    <emphasis level="strong">This is important!</emphasis>
    <say-as interpret-as="digits">2025</say-as>
  </voice>
</speak>
```
- You can nest tags: `prosody` inside `voice`, `emphasis` inside `prosody`, etc.