using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;
// based on implementation in https://github.com/radionerd/Midi-Chord-Analyser/blob/main/chord_analyser.c
public class ChordAnalysis : MonoBehaviour
{
    private static readonly string[][] KeyNotes = new string[][] 
    { 
        new[] {"C", "D♭","D", "E♭", "F♭", "F", "G♭", "G", "A♭", "A", "B♭", "C♭"}, // -7♭
        new[] {"C", "D♭","D", "E♭", "E" , "F", "G♭", "G", "A♭", "A", "B♭", "C♭"}, // -6♭
        new[] {"C" ,"D♭","D", "E♭", "E" , "F" ,"G♭", "G", "A♭", "A", "B♭", "B" }, // -5♭
        new[] {"C" ,"D♭","D","E♭","E" ,"F" ,"G♭","G","A♭","A","B♭","B" }, // -4♭
        new[] {"C" ,"D♭","D","E♭","E" ,"F" ,"G♭","G","A♭","A","B♭","B" }, // -3♭
        new[] {"C" ,"D♭","D","E♭","E" ,"F" ,"G♭","G","A♭","A","B♭","B" }, // -2♭
        new[] {"C" ,"D♭","D","E♭","E" ,"F" ,"G♭","G","A♭","A","B♭","B" }, // -1♭
        new[] {"C" ,"C♯","D","D♯","E" ,"F" ,"F♯","G","G♯","A","A♯","B" }, //  0♯
        new[] {"C" ,"C♯","D","D♯","E" ,"F" ,"F♯","G","G♯","A","A♯","B" }, //  1♯
        new[] {"C" ,"C♯","D","D♯","E" ,"F" ,"F♯","G","G♯","A","A♯","B" }, //  2♯
        new[] {"C" ,"C♯","D","D♯","E" ,"F" ,"F♯","G","G♯","A","A♯","B" }, //  3♯
        new[] {"C" ,"C♯","D","D♯","E" ,"F" ,"F♯","G","G♯","A","A♯","B" }, //  4♯
        new[] {"C" ,"C♯","D","D♯","E" ,"F" ,"F♯","G","G♯","A","A♯","B" }, //  5♯
        new[] {"C" ,"C♯","D","D♯","E" ,"E♯","F♯","G","G♯","A","A♯","B" }, //  6♯
        new[] {"B♯","C♯","D","D♯","E" ,"E♯","F♯","G","G♯","A","A♯","B" }  //  7♯
    };
    private static readonly string[] KeySf = { "7♭", "6♭", "5♭", "4♭", "3♭", "2♭", "1♭", "  ", "1♯", "2♯", "3♯", "4♯", "5♯", "6♯", "7♯" };
    private static readonly int[] KeySfIndex = { 0, -5, 2, -3, 4, -1, -6, 1, -4, 3, -2, -7 };
    private const int Major = 1;
    private const int Minor = 2;
    private const int Lowest = 4;
    private const int NewKey = 8;
    private const int LogToggle = 0x10;
    private const int MidiMiddleC = 60;
    private const int NumNotes = 128;
    private const int KeyUnknown = 2;

    private static readonly ChordDefinition[] ChordDefs = new ChordDefinition[]
    {
        new ChordDefinition(0x091, "", Major),         // C E  G    Major
        new ChordDefinition(0x891, "⁷", Major),        // C E  G  B   Major⁷
        new ChordDefinition(0x811, "⁷", Major),        // C E     B  Major⁷
        new ChordDefinition(0x491, "dom⁷", Major),     // C E  G  B♭ dominant⁷
        new ChordDefinition(0x411, "dom⁷", Major),     // C E     B♭ no 5th  dominant⁷
        new ChordDefinition(0x111, "⁺",     Lowest),   // C E  G♯  augmented
        new ChordDefinition(0x089, "m",     Minor),    // C E♭ G  minor
        new ChordDefinition(0x489, "m⁷",    Minor),    // C E  G  B♭ minor⁷
        new ChordDefinition(0x049, "°",     Minor),    // C E♭ G♭  diminished
        new ChordDefinition(0x249, "°⁷",    Lowest),   // C E♭ G♭ B♭♭ minor seventh flat five? diminished⁷
        new ChordDefinition(0x085, "sus²",  0),     // CD  G      no 3rd *** beware inversions and naming. suspended²
        new ChordDefinition(0x0A1, "sus⁵",  0),     // C  FG      no 3rd *** beware inversions and naming. suspended⁵
        new ChordDefinition(0x4a5, "9sus4", 0),     // C–F–G–B♭–D dominant9th
        new ChordDefinition(0xb01, "Log toggle", LogToggle),     // A♭ A B C
        new ChordDefinition(0xc01, "Play Major or minor chord to set new key", NewKey)  // B♭ B C
    };
    private class ChordDefinition {
        public int Notes { get; }
        public string Name { get; }
        public int Flags { get; }

        public ChordDefinition(int notes, string name, int flags)
        {
            Notes = notes;
            Name = name;
            Flags = flags;
        }
    }
    public static void ShowKeys()
    {
        Debug.Log($"{"",-12}Chord Analyser\n\n");
        Debug.Log("KSig  Major RelMinor  Diatonic Scale");

        for (int note = -49; note < 7 * 8; note += 7)
        {
            int indexMajor = (60 + note) % 12;
            int indexMinor = (indexMajor + 9) % 12;

            Debug.Log($"{KeySf[(49 + note) / 7],2}      {KeyNotes[(49 + note) / 7][indexMajor],-2}      {KeyNotes[(49 + note) / 7][indexMinor],-2}    ");

            for (int i = 0; i < 12; i++)
            {
                if (KeyNotes[7][i].Length == 1) // Detect the white keys in C Major
                    Debug.Log($"{KeyNotes[(note + 49) / 7][(60 + note + i) % 12],-2} ");
            }
            Debug.Log(" ");
        }

        Debug.Log("\nTo set the key signature play the major or minor chord of the same name.");
        Debug.Log("Play the root note above♯ or below♭ middle C to select keys with 5-7♯ or 5-7♭");
    }
    private static int RotateOctaveByN(int pattern, int n)
    {
        while (n < 0) n += 12;
        if (n >= 12) n %= 12;

        while (n > 0)
        {
            int lsb = pattern & 1;
            pattern >>= 1;
            if (lsb != 0)
                pattern |= 0x800;
            n--;
        }
        return pattern;
    }
    private static string ScaleDegree(int root, int chordId, int keyNote, bool keyIsMinor)
    {
        int[] arabicIdM = { 1, 0, 2, 0, 3, 4, 0, 5, 0, 6, 0, 7 };
        int[] arabicIdm = { 3, 0, 4, 0, 5, 6, 0, 7, 0, 1, 0, 2 };
        string[] romanMajor = { "", "I", "II", "III", "IV", "V", "VI", "VII" };
        string[] romanMinor = { "", "i", "ii", "iii", "iv", "v", "vi", "vii" };

        int chord = ChordDefs[chordId].Notes;

        if ((chord & 0x018) != 0)
        {
            int noteId = (144 + root - keyNote) % 12;
            int arabicId = keyIsMinor ? arabicIdm[(noteId + 9) % 12] : arabicIdM[noteId];

            return (chord & 0x008) != 0
                ? romanMinor[arabicId]
                : romanMajor[arabicId];
        }

        return "";
    }
        readonly static string[] offOn = { "Off", "On" };
        readonly static string[] majorMinor = { " ", "m", "" };
        int[] keyboardImage = new int[NumNotes];
        static int logEnable = 1;
        int keyNote = 0;
        int keyIsMinor = -1;
        static int numSharpsFlats = 0;
        static int lineCount = 0;
		int notes;
		int lowestNote = int.MaxValue;
		

    void ChordAnalyserMethod(int note, int velocity, int channel, bool on)
    {
		
        // Initialize keyboard image if first run
        if (keyIsMinor == -1)
        {
            Array.Fill(keyboardImage, 0);
            keyIsMinor = KeyUnknown;
        }
		notes=0;
		lowestNote=int.MaxValue;
        if (note > 0 && note < NumNotes)
        {
            keyboardImage[note] = on ? 1 : 0;

            for (int i = 0; i < NumNotes; i++)
            {
                if (keyboardImage[i] == 1)
                {
                    notes |= (1 << (i % 12));
					Debug.Log(notes);
                    if (lowestNote == int.MaxValue)
                        lowestNote = i;
                }
            }
        }
		string chordMsg="";
		string scaleDegree = "";
        for (int i = 0; i < 12; i++)
        {
            foreach (var chordDef in ChordDefs)
            {
                if (notes == chordDef.Notes)
                {
                    if ((chordDef.Flags & NewKey) != 0)
                    {
                        if (lowestNote > 91)
                        {
                            chordMsg = "Play Major or minor chord to set new key";
                            keyIsMinor = KeyUnknown;
                            keyNote = 12;
                        }
                    }
                    else if ((chordDef.Flags & LogToggle) != 0)
                    {
                        if (lowestNote > 91)
                        {
                            logEnable ^= 1;
                            chordMsg = $"Log = {offOn[logEnable]}";
                        }
                    }
                    else
                    {
                        if (keyIsMinor == KeyUnknown && 
                            ((chordDef.Flags & Major) != 0 || (chordDef.Flags & Minor) != 0))
                        {
                            keyNote = lowestNote;
                            keyIsMinor = (chordDef.Flags & Minor) != 0 ? 1 : 0;
                            numSharpsFlats = KeySfIndex[(keyNote - keyIsMinor * 9) % 12];

                            if (numSharpsFlats <= -5 && lowestNote >= MidiMiddleC)
                                numSharpsFlats += 12;

                            lineCount = 0;
                        }

                        int noteId = i;
                        if ((chordDef.Flags & Lowest) != 0)
                            noteId = lowestNote % 12;

                        if ((chordDef.Flags & Major) != 0 || (chordDef.Flags & Minor) != 0)
                            scaleDegree = ScaleDegree(noteId, Array.IndexOf(ChordDefs, chordDef), keyNote, keyIsMinor == 1);

                        chordMsg = $"{KeyNotes[numSharpsFlats + 7][noteId]}{chordDef.Name}";

                        if (noteId != lowestNote % 12)
                            chordMsg += $"/{KeyNotes[numSharpsFlats + 7][lowestNote % 12]} ";

                    }
                }
            }
	        notes = RotateOctaveByN(notes, 1);
        }
		

        if (logEnable == 1)
        {
            Debug.Log($"Midi note={note}, mask={1 << note % 12:X3}, notes={notes:X3} {chordMsg}");
        }
        else
        {
            if (keyIsMinor == KeyUnknown)
            {
                if (lineCount >= 0)
                {
                    lineCount = -1;
                    //ShowKeys();
					Debug.Log("keyIsMinor == KeyUnknown");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(chordMsg))
                {
                    if (--lineCount <= 0)
                    {
                        lineCount = 20;
                        Debug.Log("Key    Key   Scale");
                        Debug.Log("Sig    Name  Degree   Chord");
                    }

                    if (string.IsNullOrEmpty(scaleDegree))
                        scaleDegree = "    ";

                    Debug.Log($"{KeySf[numSharpsFlats + 7],2}    {KeyNotes[numSharpsFlats + 7][keyNote % 12] + majorMinor[keyIsMinor],3}    {scaleDegree,4}     {chordMsg}");
                }
            }
        }
    }
    // yeah
    MidiFilePlayer mfp;

    // Start is called before the first frame update
    void Start()
    {
        mfp = FindFirstObjectByType<MidiFilePlayer>();
        mfp.OnEventNotesMidi.AddListener(NotesToPlay);
    }

    public void NotesToPlay(List<MPTKEvent> mptkEvents)
    {
        //Debug.Log("Recieved " + mptkEvents.Count + " MIDI Events");
        foreach (MPTKEvent mptkEvent in mptkEvents)
        {
            if (mptkEvent.Command == MPTKCommand.NoteOn)
            {
                ChordAnalyserMethod(mptkEvent.Value, mptkEvent.Velocity, mptkEvent.Channel, true);
                //Debug.Log(mptkEvent.Value);
            }
            //else if (mptkEvent.Command == MPTKCommand.NoteOff)
            //    ChordAnalyserMethod(mptkEvent.Value, mptkEvent.Velocity, mptkEvent.Channel, false);
                //Debug.Log(mptkEvent.Value);
                
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
