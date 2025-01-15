using UnityEngine;
using UnityEngine.UIElements;
using MidiPlayerTK;

public class SimpleRuntimeUI : MonoBehaviour
{
    public MidiFilePlayer midiFilePlayer;
    private Button _button;
    void Start()
    {
        midiFilePlayer.MPTK_MidiName = "basic";
        //midiFilePlayer.MPTK_Play();
        var uiDocument = GetComponent<UIDocument>();
        VisualElement root = uiDocument.rootVisualElement;
        Label filename = new Label("MIDI file name: " + midiFilePlayer.MPTK_MidiName);
        root.Add(filename);
        Label tempoTime = new Label("Tempo: " + midiFilePlayer.MPTK_Tempo + " Song length: " + midiFilePlayer.MPTK_Duration);
        root.Add(tempoTime);
    }

    //Add logic that interacts with the UI controls in the `OnEnable` methods
    private void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();

        _button = uiDocument.rootVisualElement.Q("button") as Button;

        _button.RegisterCallback<ClickEvent>(PlayMidi);

        var _inputFields = uiDocument.rootVisualElement.Q("input-message");
        _inputFields.RegisterCallback<ChangeEvent<string>>(InputMessage);
    }

    private void OnDisable()
    {
        _button.UnregisterCallback<ClickEvent>(PrintClickMessage);
    }
    private void PlayMidi(ClickEvent evt)
    {
        midiFilePlayer.MPTK_Play();
    }
    private void PrintClickMessage(ClickEvent evt)
    {
        var uiDocument = GetComponent<UIDocument>();
        VisualElement root = uiDocument.rootVisualElement;
        midiFilePlayer.MPTK_Stop();
        midiFilePlayer.MPTK_MidiName = "its been so long";
        midiFilePlayer.MPTK_Play();
        Label filename = new Label("MIDI file name: " + midiFilePlayer.MPTK_MidiName);
        root.Clear();
        Label tempoTime = new Label("Tempo: " + midiFilePlayer.MPTK_Tempo + " Song length: " + midiFilePlayer.MPTK_Duration);
        root.Add(filename);
        root.Add(tempoTime);
    }

    public void InputMessage(ChangeEvent<string> evt)
    {
        midiFilePlayer.MPTK_MidiName = $"{evt.newValue}";
    }
}
