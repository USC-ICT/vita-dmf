using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class MenuManagerPanelLogin : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    MenuManager m_menuManager;
    InputField m_username;
    InputField m_password;
    GameObject m_version;
    Button m_loginButton;
    GameObject m_loginLoadingIcon;

    void Start()
    {
        m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.Login);
        m_username = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_UserName").GetComponent<InputField>();
        m_password = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_Password").GetComponent<InputField>();
        m_version = VHUtils.FindChildRecursive(menu, "Text_VersionNumber");
        m_loginButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_Login").GetComponent<Button>();
        m_loginLoadingIcon = VHUtils.FindChildRecursive(menu, "GuiIconLoadingPrefab_Login");
    }

    public void OnMenuEnter()
    {
        if (PlayerPrefs.HasKey("vitaLastUsername") &&
            PlayerPrefs.HasKey("vitaLastPassword"))
        {
            m_username.text = PlayerPrefs.GetString("vitaLastUsername");
            m_password.text = PlayerPrefs.GetString("vitaLastPassword");
        }

        m_version.GetComponent<Text>().text = VitaGlobals.m_version;
        m_loginButton.interactable = true;
        m_loginLoadingIcon.SetActive(false);
    }

    public void OnMenuExit()
    {
    }

    public void Login()
    {
        m_loginButton.interactable = false;
        m_loginLoadingIcon.SetActive(true);

        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.Login(VitaGlobals.m_statusUsername, VitaGlobals.m_statusPassword, (statusProfile, errorA) =>
        {
            if (!string.IsNullOrEmpty(errorA))
            {
                m_loginButton.interactable = true;
                m_loginLoadingIcon.SetActive(false);
                PopUpDisplay.Instance.Display("Error", errorA);
                return;
            }

            if (statusProfile == null)
            {
                m_loginButton.interactable = true;
                m_loginLoadingIcon.SetActive(false);
                PopUpDisplay.Instance.Display("Error", errorA);
                return;
            }

            if (statusProfile.name != VitaGlobals.m_statusRunning)
            {
                m_loginButton.interactable = true;
                m_loginLoadingIcon.SetActive(false);
                PopUpDisplay.Instance.Display("Database down", "ViTA database down for maintenance.  Please wait for the database to come back online.");
                return;
            }

            dbUser.Login(VitaGlobals.m_versionUsername, VitaGlobals.m_versionPassword, (versionProfile, error) =>
            {
                if (!string.IsNullOrEmpty(error))
                {
                    m_loginButton.interactable = true;
                    m_loginLoadingIcon.SetActive(false);
                    PopUpDisplay.Instance.Display("Error", error);
                    return;
                }

                if (versionProfile == null)
                {
                    m_loginButton.interactable = true;
                    m_loginLoadingIcon.SetActive(false);
                    PopUpDisplay.Instance.Display("Error", error);
                    return;
                }

                if (!VitaGlobals.VersionClientServerCompatible(versionProfile.name))
                {
                    m_loginButton.interactable = true;
                    m_loginLoadingIcon.SetActive(false);
                    PopUpDisplay.Instance.Display("Update Required", "VITA application requires an updated version.  Please contact DMF to obtain the newest version.");
                    return;
                }

                dbUser.Login(m_username.text, m_password.text, (profile, error2) =>
                {
                    if (!string.IsNullOrEmpty(error2))
                    {
                        m_loginButton.interactable = true;
                        m_loginLoadingIcon.SetActive(false);
                        if (error2.Contains("No item found: EntityDBVitaProfile"))
                        {
                            PopUpDisplay.Instance.Display("Error", "Invalid user name or password.\n \n For further assistance, please contact your System Administrator.");
                        }
                        else
                        {
                            PopUpDisplay.Instance.Display("Error", error2);
                        }
                        return;
                    }

                    if (profile == null)
                    {
                        m_loginButton.interactable = true;
                        m_loginLoadingIcon.SetActive(false);
                        PopUpDisplay.Instance.Display("Error", error2);
                        return;
                    }

                    DBOrganization dbOrganization = GameObject.FindObjectOfType<DBOrganization>();
                    dbOrganization.GetOrganization(profile.organization, (organization, error3) =>
                    {
                        if (!string.IsNullOrEmpty(error3))
                        {
                            m_loginButton.interactable = true;
                            m_loginLoadingIcon.SetActive(false); 
                            PopUpDisplay.Instance.Display("Error", error2);
                            return;
                        }

                        if (!IsValidUser(profile, organization))
                        {
                            m_loginButton.interactable = true;
                            m_loginLoadingIcon.SetActive(false);
                            PopUpDisplay.Instance.Display("Error", "This is not a valid user.  Please try again.\n \n For further assistance, please contact your System Administrator.");
                            return;
                        }

                        VitaGlobals.m_loggedInProfile = profile;
                        VitaGlobals.m_interviewStudent = "";
                        VitaGlobals.m_selectedSession = "";

                        if (profile.type == 0)      { m_menuManager.ChangeMenu(MenuManager.Menu.DMFAdminHub); }
                        else if (profile.type == 1) { m_menuManager.ChangeMenu(MenuManager.Menu.LocalAdminHubTeachers); }
                        else if (profile.type == 2) { m_menuManager.ChangeMenu(MenuManager.Menu.TeacherConfigure); }
                        else if (profile.type == 3) { m_menuManager.ChangeMenu(MenuManager.Menu.StudentSessionConfiguration); }

                        PlayerPrefs.SetString("vitaLastUsername", m_username.text);
                        PlayerPrefs.SetString("vitaLastPassword", profile.password);

                        dbUser.UpdateUserLastLogin(VitaGlobals.m_loggedInProfile, VitaGlobals.CurrentTimeToString(), (error4) => 
                        {
                            if (!string.IsNullOrEmpty(error4))
                            {
                                PopUpDisplay.Instance.Display("Error", error4);
                                return;
                            }

                            if (!dbUser.DoesUserHaveBadge(VitaGlobals.m_loggedInProfile, "First Step"))
                            {
                                dbUser.AddBadge(VitaGlobals.m_loggedInProfile.username, DBUser.BadgeData.NewBadge("First Step"), (newProfile, error5) => 
                                {
                                    if (!string.IsNullOrEmpty(error5))
                                    {
                                        PopUpDisplay.Instance.Display("Error", error5);
                                        return;
                                    }

                                    VitaGlobals.m_loggedInProfile = newProfile;
                                });
                            }

                            DBSession dbSession = GameObject.FindObjectOfType<DBSession>();
                            dbSession.AddEvent("GLOBAL", VitaGlobals.m_loggedInProfile.username, DBSession.EventData.NewLogin(), error6 =>
                            {
                                if (!string.IsNullOrEmpty(error6))
                                {
                                    PopUpDisplay.Instance.Display("Error", error6);
                                    return;
                                }
                            });

                            m_loginButton.interactable = true;
                            m_loginLoadingIcon.SetActive(false);
                        });
                    });
                });
            });
        });
    }

    public void BtnDemonstration()
    {
        m_menuManager.ChangeMenu(MenuManager.Menu.DemoConfigure);
    }

    public void BtnQuit()
    {
        NetworkRelay.SendNetworkMessage("quit");
    }


    bool IsValidUser(EntityDBVitaProfile profile, EntityDBVitaOrganization organization)
    {
        if (profile == null)
            return false;

        if (profile.archived > 0)
            return false;

        if (organization == null)
            return false;

        if (organization.archived > 0)
            return false;

        DateTime expiredateProfile = VitaGlobals.TicksToDateTime(profile.expiredate);
        if (DateTime.Now > expiredateProfile)
            return false;

        DateTime expiredateOrg = VitaGlobals.TicksToDateTime(organization.accexpire);
        if (DateTime.Now > expiredateOrg)
            return false;

        return true;
    }
}
