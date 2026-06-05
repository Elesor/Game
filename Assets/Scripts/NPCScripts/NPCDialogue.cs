using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "NPC Dialogue")]

public class NPCDialogue : ScriptableObject
{
    public string npcName;
    public Sprite npcPortrait;
    public string[] dialoguelines;
    public bool[] autoProgressLines;
    public bool[] endgDialoguesLines;
    public float autoProgressDelay = 1.5f;
    public float typingSpeed = 0.05f;
    public AudioClip voicesound;
    public float voicepath = 1f;

    public DialogueChoice[] choices;

    public int questInProgressIndex;
    public int questCompletedIndex;
    public Quest quest;
}
[System.Serializable]

public class DialogueChoice
{
    public int dialogueIndex;
    public string[] choisces;
    public int[] nextDialogueIndexes;
    public bool[] givesQuest;
}
