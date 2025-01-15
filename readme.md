# UnityMusicVisualizer
Currently the program determines the song's key based on the first chord played, as the code was originally intended for live MIDI playback. I aim to re-implement this functionality in UIToolkit so users may input a song's key, in case the first chord is not the I chord.
After detecting the key, chords are analyzed to determine the chord itself, followed by its type and how it fits within the key.
The detection needs some work, as does the output (currently only to the Unity Editor's Console) but it should not be difficult to integrate it within Lucas's Isochord visualization.

## Utilization
To use the UnityMusicVisualizer, open the project in the Editor. In the menu bar, choose Maestro -> Midi File Setup. By default the project includes a midi file with two F minor chords, titled "f minor chords".
Users can upload their own files into this window, but they will need to type in that file name within the textbox in the game window. In addition, a file must start with a I or i chord for key detection to function currently.
Click "Play" to play the MIDI file, and monitor the Console to see which chords are played.

References:
https://github.com/radionerd/Midi-Chord-Analyser/
