using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MecanimManager : ICharacterController
{
    #region Constants
    public static readonly Dictionary<string, string[]> VisemeMap = new Dictionary<string, string[]>()
    {
        {"Face_neutral",                new string[] {"Face_neutral"} },
        {"FV" ,                         new string[] {"FV"} },
        {"open" ,                       new string[] {"open"} },
        {"PBM" ,                        new string[] {"PBM"} },
        {"ShCh" ,                       new string[] {"ShCh"} },
        {"tBack" ,                      new string[] {"tBack"} },
        {"tRoof" ,                      new string[] {"tRoof"} },
        {"tTeeth" ,                     new string[] {"tTeeth"} },
        {"W" ,                          new string[] {"W"} },
        {"wide",                        new string[] {"wide"} },

        {"au_1",                        new string[] {"001_inner_brow_raiser_lf", "001_inner_brow_raiser_rt"} },
        {"au_2",                        new string[] {"002_outer_brow_raiser_lf", "002_outer_brow_raiser_rt"} },
        {"au_4",                        new string[] {"004_brow_lowerer_lf", "004_brow_lowerer_rt"} },
        {"au_5",                        new string[] {"005_upper_lid_raiser"} },
        {"au_6",                        new string[] {"006_cheek_raiser"} },
        {"au_7",                        new string[] {"007_lid_tightener"} },
        {"au_10",                       new string[] {"010_upper_lip_raiser"} },
        {"au_12",                       new string[] {"012_lip_corner_puller_lf", "012_lip_corner_puller_rt"} },
        {"au_14",                       new string[] {"014_smile_lf", "014_smile_rt"} },
        {"au_25",                       new string[] {"025_lips_part"} },
        {"au_26",                       new string[] {"026_jaw_drop"} },
        {"au_45",                       new string[] {"045_blink_lf", "045_blink_rt"} },
        {"au_100",                      new string[] {"100_small_smile"} },
        {"au_112",                      new string[] {"112_happy"} },
        {"au_124",                      new string[] {"124_disgust"} },
        {"au_126",                      new string[] {"126_fear"} },
        {"au_127",                      new string[] {"127_surprise"} },
        {"au_129",                      new string[] {"129_angry"} },
        {"au_130",                      new string[] {"130_sad"} },
        {"au_131",                      new string[] {"131_contempt"} },
        {"au_132",                      new string[] {"132_browraise1"} },
        {"au_133",                      new string[] {"133_browraise2"} },
        {"au_134",                      new string[] {"134_hurt_brows"} },
        {"au_136",                      new string[] {"136_furrow"} },
    };
    #endregion

    #region Variables
    [SerializeField] List<MecanimCharacter> m_characterList = new List<MecanimCharacter>();
    AudioSpeechFile[] m_SpeechFiles;
    static MecanimManager g_instance;
    #endregion

    #region Functions
    public static MecanimManager Get()
    {
        //Debug.Log("SmartbodyManager.Get()");
        if (g_instance == null)
        {
            g_instance = UnityEngine.Object.FindObjectOfType(typeof(MecanimManager)) as MecanimManager;
        }

        return g_instance;
    }

    void Awake()
    {
        MecanimCharacter[] mecanimCharacters = (MecanimCharacter[])GameObject.FindObjectsOfType(typeof(MecanimCharacter));
        foreach (MecanimCharacter mecAnimCharacter in mecanimCharacters)
        {
            AddCharacter(mecAnimCharacter);
        }

        m_SpeechFiles = (AudioSpeechFile[])GameObject.FindObjectsOfType(typeof(AudioSpeechFile));
    }

    public void AddCharacter(MecanimCharacter mecAnimCharacter)
    {
        if (!m_characterList.Contains(mecAnimCharacter))
        {
            m_characterList.Add(mecAnimCharacter);
        }
    }

    public void RemoveCharacter(MecanimCharacter mecAnimCharacter)
    {
        m_characterList.Remove(mecAnimCharacter);
    }

    public void RemoveCharacter(string character)
    {
        MecanimCharacter chr = GetCharacterByName(character);
        if (chr != null)
        {
            RemoveCharacter(chr);
        }
        else
        {
            Debug.LogError("Can't find mecanim character " + character);
        }
    }

    public void RemoveAllCharacters()
    {
        while (m_characterList.Count > 0)
        {
            RemoveCharacter(m_characterList[0]);
        }
    }

    public MecanimCharacter GetCharacterByName(string character)
    {
        MecanimCharacter ch = m_characterList.Find(delegate (MecanimCharacter c) { return c.CharacterName == character; });
        if (ch == null)
        {
            Debug.LogError("Can't find character " + character + " in ICharacterController " + name);
        }
        return ch;
    }

    GameObject GetPawn(string pawnName)
    {
        GameObject pawn = GameObject.Find(pawnName);

        if (pawn == null)
        {
            Debug.LogError(name + " can't find pawn with name: " + pawnName);
        }
        return pawn;
    }

    AudioSpeechFile GetSpeechFile(string fileName)
    {
        AudioSpeechFile speechFile = Array.Find<AudioSpeechFile>(m_SpeechFiles, s => s.name == fileName);
        if (speechFile == null)
        {
            Debug.LogError("Can't find AudioSpeechFile: " + fileName);
        }
        return speechFile;
    }

    public static string[] GetEffectedFaceParameterNames(string viseme)
    {
        string[] effectedVisemes = null;
        if (VisemeMap.ContainsKey(viseme))
        {
            effectedVisemes = VisemeMap[viseme];
        }
        return effectedVisemes;
    }

    public AudioSource GetCharacterVoice(string character)
    {
        AudioSource src = null;
        MecanimCharacter ch = GetCharacterByName(character);
        if (ch != null)
        {
            src = ch.Voice;
        }
        return src;
    }

    void PlayAudioClip(string character, AudioClip clip)
    {
        AudioSource src = GetCharacterVoice(character);
        if (src != null)
        {
            src.clip = clip;
            src.Play();
        }
    }

    public void SetCharacterFloatParam(string character, string paramName, float paramData)
    {
        GetCharacterByName(character).SetFloatParam(paramName, paramData);
    }

    public void SetCharacterBoolParam(string character, string paramName, bool paramData)
    {
        GetCharacterByName(character).SetBoolParam(paramName, paramData);
    }

    public void SetCharacterIntParam(string character, string paramName, int paramData)
    {
        GetCharacterByName(character).SetIntParam(paramName, paramData);
    }
    #endregion

    #region ICharacterController Functions
    public override void SBRunPythonScript(string script)
    {
        /*string command = string.Format(@"scene.run('{0}')", script);
        PythonCommand(command);*/
    }

    public override void SBMoveCharacter(string character, string direction, float fSpeed, float fLrps, float fFadeOutTime)
    {
        /*string command = string.Format(@"scene.command('sbm test loco char {0} {1} spd {2} rps {3} time {4}')", character, direction, fSpeed, fLrps, fFadeOutTime);
        PythonCommand(command);*/
    }

    public override void SBWalkTo(string character, string waypoint, bool isRunning)
    {
        /*string run = isRunning ? @"manner=""run""" : "";
        string message = string.Format(@"bml.execBML('{0}', '<locomotion target=""{1}"" facing=""{2}"" {3} />')", character, waypoint, waypoint, run);
        PythonCommand(message);*/
    }

    public override void SBWalkImmediate(string character, string locomotionPrefix, double velocity, double turn, double strafe)
    {
        //<sbm:states mode="schedule" loop="true" name="allLocomotion" x="100" y ="0" z="0"/>
        /*string message = string.Format(@"bml.execBML('{0}', '<sbm:states mode=""schedule"" loop=""true"" sbm:startnow=""true"" name=""{1}"" x=""{2}"" y =""{3}"" z=""{4}"" />')", character, locomotionPrefix, velocity, turn, strafe);
        PythonCommand(message);*/
    }

    public override void SBPlayAudio(string character, string audioId)
    {
        AudioSpeechFile speechFile = GetSpeechFile(audioId);
        if (speechFile != null)
        {
            GetCharacterByName(character).PlayAudio(speechFile);
        }
    }

    public override void SBPlayAudio(string character, string audioId, string text)
    {
        AudioSpeechFile speechFile = GetSpeechFile(audioId);
        if (speechFile != null)
        {
            GetCharacterByName(character).PlayAudio(speechFile);
        }
    }

    public override void SBPlayAudio(string character, AudioClip audioId)
    {
        AudioSpeechFile speechFile = GetSpeechFile(audioId.name);
        if (speechFile != null)
        {
            GetCharacterByName(character).PlayAudio(speechFile);
        }
    }

    public override void SBPlayAudio(string character, AudioClip audioId, string text)
    {
        AudioSpeechFile speechFile = GetSpeechFile(audioId.name);
        if (speechFile != null)
        {
            GetCharacterByName(character).PlayAudio(speechFile);
        }
    }

    public override void SBPlayAudio(string character, AudioSpeechFile audioId)
    {
        GetCharacterByName(character).PlayAudio(audioId);
    }

    public override void SBPlayXml(string character, string xml)
    {
        GetCharacterByName(character).PlayXml(xml);
    }

    public override void SBPlayXml(string character, AudioSpeechFile xml)
    {
        GetCharacterByName(character).PlayXml(xml.ConvertedXml);
    }

    public override void SBTransform(string character, Transform transform)
    {
        SBTransform(character, transform.position, transform.rotation);
    }

    public override void SBTransform(string character, Vector3 pos, Quaternion rot)
    {
        MecanimCharacter c = GetCharacterByName(character);
        c.transform.localPosition = pos;
        c.transform.localRotation = rot;
    }

    public override void SBTransform(string character, double y, double p)
    {

    }

    public override void SBTransform(string character, double x, double y, double z)
    {
        GetCharacterByName(character).transform.position = new Vector3((float)x, (float)y, (float)z);
    }

    public override void SBTransform(string character, double x, double y, double z, double h, double p, double r)
    {
        SBTransform(character, new Vector3((float)x, (float)y, (float)z), Quaternion.Euler(new Vector3((float)p, (float)h, (float)r)));
    }

    public override void SBRotate(string character, double h)
    {
        GetCharacterByName(character).transform.Rotate(0, (float)h, 0);
    }

    public override void SBPosture(string character, string posture, float startTime)
    {
        GetCharacterByName(character).PlayPosture(posture);
    }

    public override void SBPlayAnim(string character, string animName)
    {
        /*string message = string.Format(@"bml.execBML('{0}', '<animation name=""{1}""/>')", character, animName);
        PythonCommand(message);*/
        GetCharacterByName(character).PlayAnim(animName);
    }

    public override void SBPlayAnim(string character, string animName, float readyTime, float strokeStartTime, float emphasisTime, float strokeTime, float relaxTime)
    {
        SBPlayAnim(character, animName);
    }

    public override void SBPlayFAC(string character, int au, CharacterDefines.FaceSide side, float weight, float time)
    {
        string fac = "au_" + au.ToString();
        if (VisemeMap.ContainsKey(fac))
        {
            string[] facNames = VisemeMap[fac];
            string leftName, rightName;

            ParseFacLeftRightNames(facNames, out leftName, out rightName);

            if ((side == CharacterDefines.FaceSide.left || side == CharacterDefines.FaceSide.both) && !string.IsNullOrEmpty(leftName))
            {
                GetCharacterByName(character).PlayViseme(leftName, weight, time, 0.25f); // smartbody default
            }
            if ((side == CharacterDefines.FaceSide.right || side == CharacterDefines.FaceSide.both) && !string.IsNullOrEmpty(rightName))
            {
                GetCharacterByName(character).PlayViseme(rightName, weight, time, 0.25f); // smartbody default
            }
        }
        else
        {
            Debug.LogError("au " + au + " doesn't exist in the VisemeMap");
        }
    }

    void ParseFacLeftRightNames(string[] facNames, out string left, out string right)
    {
        left = right = string.Empty;
        left = Array.Find<string>(facNames, s => s.Contains("_lf"));
        right = Array.Find<string>(facNames, s => s.Contains("_rt"));
        if (string.IsNullOrEmpty(left))
        {
            left = facNames[0];
        }
        if (string.IsNullOrEmpty(right))
        {
            right = facNames[0];
        }
    }

    public override void SBPlayViseme(string character, string viseme, float weight)
    {
        GetCharacterByName(character).PlayViseme(viseme, weight);
    }

    public override void SBPlayViseme(string character, string viseme, float weight, float totalTime, float blendTime)
    {
        GetCharacterByName(character).PlayViseme(viseme, weight, totalTime, blendTime);
    }

    public void SetVisemeWeightMultiplier(string character,float multiplier)
    {
        MecanimCharacter chr = GetCharacterByName(character);
        if (chr!= null)
        {
            chr.SetVisemeWeightMultiplier(multiplier);
        }
    }

    public override void SBNod(string character, float amount, float repeats, float time)
    {
        GetCharacterByName(character).Nod(amount, repeats, time);
    }

    public override void SBShake(string character, float amount, float repeats, float time)
    {
        GetCharacterByName(character).Shake(amount, repeats, time);
    }

    public void Tilt(string character, float amount, float repeats, float time)
    {
        GetCharacterByName(character).Tilt(amount, repeats, time);
    }

    public override void SBGaze(string character, string gazeAt)
    {
        GameObject pawn = GetPawn(gazeAt);
        if (pawn != null)
        {
            GetCharacterByName(character).SetGazeTarget(pawn);
        }
    }

    public override void SBGaze(string character, string gazeAt, float neckSpeed)
    {
        GameObject pawn = GetPawn(gazeAt);
        if (pawn != null)
        {
            MecanimCharacter c = GetCharacterByName(character);
            c.Gaze(gazeAt, neckSpeed);
        }
    }

    public override void SBGaze(string character, string gazeAt, float neckSpeed, float eyeSpeed, CharacterDefines.GazeJointRange jointRange)
    {
        GameObject pawn = GetPawn(gazeAt);
        if (pawn != null)
        {
            MecanimCharacter c = GetCharacterByName(character);
            c.Gaze(gazeAt, neckSpeed, eyeSpeed, jointRange);
        }
    }

    public override void SBGaze(string character, string gazeAt, string targetBone, CharacterDefines.GazeDirection gazeDirection,
        CharacterDefines.GazeJointRange jointRange, float angle, float headSpeed, float eyeSpeed, float fadeOut, string gazeHandleName, float duration)
    {
        GameObject pawn = GetPawn(gazeAt);
        if (pawn != null)
        {
            MecanimCharacter c = GetCharacterByName(character);
            c.Gaze(gazeAt, headSpeed, eyeSpeed, jointRange);
            if (duration > 0)
            {
                c.StopGazeLater(duration, fadeOut);
            }
        }
    }

    public void GazeSpeed(string character, string gazeAt, float headSpeed, float eyesSpeed, float bodySpeed)
    {
        GameObject pawn = GetPawn(gazeAt);
        if (pawn != null)
        {
            GetCharacterByName(character).SetGazeTargetWithSpeed(pawn, headSpeed, eyesSpeed, bodySpeed);
        }
    }

    public void GazeTime(string character, string gazeAt, float headFadeInTime, float eyesFadeInTime, float bodyFadeInTime)
    {
        GameObject pawn = GetPawn(gazeAt);
        if (pawn != null)
        {
            GetCharacterByName(character).SetGazeTargetWithTime(pawn, headFadeInTime, eyesFadeInTime, bodyFadeInTime);
        }
    }

    public void SetGazeWeights(string character, float head, float eyes, float body)
    {
        GetCharacterByName(character).SetGazeWeights(head, eyes, body);
    }

    public override void SBStopGaze(string character)
    {
        GetCharacterByName(character).StopGaze();
    }

    public override void SBStopGaze(string character, float fadoutTime)
    {
        GetCharacterByName(character).StopGaze(fadoutTime);
    }

    public override void SBSaccade(string character, CharacterDefines.SaccadeType type, bool finish, float duration)
    {
        GetCharacterByName(character).SetSaccadeBehaviour(type);
    }

    public override void SBSaccade(string character, CharacterDefines.SaccadeType type, bool finish, float duration, float angleLimit, float direction, float magnitude)
    {
        GetCharacterByName(character).Saccade(direction, magnitude, duration);
    }

    public override void SBStopSaccade(string character)
    {
        GetCharacterByName(character).StopSaccade();
    }

    public void SetSaccadeBehaviour(string character, CharacterDefines.SaccadeType behaviour)
    {
        GetCharacterByName(character).SetSaccadeBehaviour(behaviour);
    }

    public void PlaySaccade(string character, float direction, float magnitude, float duration)
    {
        GetCharacterByName(character).Saccade(direction, magnitude, duration);
    }

    public override void SBStateChange(string character, string state, string mode, string wrapMode, string scheduleMode)
    {
        /*string message = string.Format(@"bml.execBML('{0}', '<sbm:states name=""{1}"" mode=""{2}"" sbm:wrap-mode=""{3}"" sbm:schedule-mode=""{4}""/>')", character, state, mode, wrapMode, scheduleMode);
        PythonCommand(message);*/
    }

    public override void SBStateChange(string character, string state, string mode, string wrapMode, string scheduleMode, float x)
    {
        /*string message = string.Format(@"bml.execBML('{0}', '<sbm:states name=""{1}"" mode=""{2}"" sbm:wrap-mode=""{3}"" sbm:schedule-mode=""{4}"" x=""{5}""/>')", character, state, mode, wrapMode, scheduleMode, x.ToString());
        PythonCommand(message);*/
    }

    public override void SBStateChange(string character, string state, string mode, string wrapMode, string scheduleMode, float x, float y, float z)
    {
        /*string message = string.Format(@"bml.execBML('{0}', '<sbm:states name=""{1}"" mode=""{2}"" sbm:wrap-mode=""{3}"" sbm:schedule-mode=""{4}"" x=""{5}"" y=""{6}"" z=""{7}""/>')", character, state, mode, wrapMode, scheduleMode, x.ToString(), y.ToString(), z.ToString());
        PythonCommand(message);*/
    }

    public override string SBGetCurrentStateName(string character)
    {
        /*string pyCmd = string.Format(@"scene.getStateManager().getCurrentState('{0}')", character);
        return PythonCommand<string>(pyCmd);*/
        return string.Empty;
    }

    public override Vector3 SBGetCurrentStateParams(string character)
    {
        /*Vector3 ret = new Vector3();
        string pyCmd = string.Empty;

        pyCmd = string.Format(@"scene.getStateManager().getCurrentStateParameters('{0}').getData(0)", character);
        ret.x = PythonCommand<float>(pyCmd);

        pyCmd = string.Format(@"scene.getStateManager().getCurrentStateParameters('{0}').getData(1)", character);
        ret.y = PythonCommand<float>(pyCmd);

        pyCmd = string.Format(@"scene.getStateManager().getCurrentStateParameters('{0}').getData(2)", character);
        ret.z = PythonCommand<float>(pyCmd);

        return ret;*/
        return Vector3.zero;
    }

    public override bool SBIsStateScheduled(string character, string stateName)
    {
        /*string pyCmd = string.Format(@"scene.getStateManager().isStateScheduled('{0}', '{1}')", character, stateName);
        return PythonCommand<bool>(pyCmd);*/
        return false;
    }

    public override float SBGetAuValue(string character, string auName)
    {
        /*string pyCmd = string.Format(@"scene.getCharacter('{0}').getSkeleton().getJointByName('{1}').getPosition().getData(0)", character, auName);
        return PythonCommand<float>(pyCmd);*/
        return 0;
    }

    public override void SBExpress(string character, string uttID, string uttNum, string text)
    {
        SBExpress(character, uttID, uttNum, text, "user");
    }

    public override void SBExpress(string character, string uttID, string uttNum, string text, string target)
    {
        /*string message = string.Format("vrExpress {0} user 1303332588320-{2}-1 <?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\" ?>"
            + "<act><participant id=\"{0}\" role=\"actor\" /><fml><turn start=\"take\" end=\"give\" /><affect type=\"neutral\" "
            + "target=\"addressee\"></affect><culture type=\"neutral\"></culture><personality type=\"neutral\"></personality></fml>"
            + "<bml><speech id=\"sp1\" ref=\"{1}\" type=\"application/ssml+xml\">{3}</speech></bml></act>", character, uttID, uttNum, text);
        PythonCommand(message);*/
    }

    public override void SBGesture(string character, string gestureName)
    {

    }

    public override void SBGesture(string character, string lexeme, string lexemeType, GestureUtils.Handedness hand, GestureUtils.Style style, GestureUtils.Emotion emotion,
            string target, bool additive, string jointRange, float perlinFrequency, float perlinScale, float readyTime, float strokeStartTime,
        float emphasisTime, float strokeTime, float relaxTime)
    {

    }

    public override ICharacter[] GetControlledCharacters ()
    {
        return m_characterList.ToArray();
    }

    public override ICharacter GetCharacter (string character)
    {
        return GetCharacterByName(character);
    }
    #endregion
}
