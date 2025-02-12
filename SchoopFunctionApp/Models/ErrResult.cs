using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SchoopFunctionApp.Models
{
    public class ErrResult
    {
        public string res;

        public ErrResult(string m_res)
        {
            res = m_res;
        }

        public ErrResult()
        {
            // TODO: Complete member initialization
        }
    }

    //====================================================================
    // This is for creatign a JSOn object that passes the new device ID
    // created in the database to the device. This is our UUID for
    // the device in our App. Gets around the fact that Google, Apple etc
    // change the UUID to avoid tracking
    //====================================================================

    public class clsDeviceID
    {
        public string res;

        public string deviceID;

        public clsDeviceID(string m_res, string m_deviceID)
        {
            res = m_res;
            deviceID = m_deviceID;
        }
    }

    //=====================================================================
    // Class School is for passing JSOON object of School details to device
    //=====================================================================

    public class School
    {
        // Result value = 1 for success
        public string res;

        // School GUID for use as primary key
        public int schoopID;

        // School Name
        public string sch1;

        // School Street
        public string sch2;

        // School Locality
        public string sch3;

        // School Address 3
        public string sch4;

        // School Town / City
        public string sch5;

        // School Postcode
        public string sch6;

        // Lowest age of students
        public string sLow;

        // Highest age of students
        public string sHigh;

        // Telephone number
        public string schTel;

        //School email
        public string schEmail;

        //Website
        public string schWeb;

        // Head teacher
        public string schHead;

        //Is active Schoop Subscriber
        public bool schActive;

        public bool hasYears;
        public bool hasGroups;
        public string organisationTypeName;

        public string activeYears;
        public string activeGroups;
        public string channelDescription;
        public string channelLogo;

        public string latitude;
        public string longitude;

        public School()
        {

        }

        public School(string m_res, int m_schguid, string m_schoolName, string m_schoolStreet, string m_schoolLocality, string m_schoolAddress3, string m_schooltown, string m_schoolpostcode, string m_sLow, string m_sHigh, string m_tel, string m_email, string m_web, string m_head, bool m_active, bool m_hasYears, bool m_hasGroups, string m_organisationTypeName)
        {
            res = m_res; // please leave this as is. Paul 30/01/2013
            schoopID = m_schguid;
            sch1 = string.IsNullOrEmpty(m_schoolName) ? "" : m_schoolName;
            sch2 = string.IsNullOrEmpty(m_schoolStreet) ? "" : m_schoolStreet;
            sch3 = string.IsNullOrEmpty(m_schoolLocality) ? "" : m_schoolLocality;
            sch4 = string.IsNullOrEmpty(m_schoolAddress3) ? "" : m_schoolAddress3;
            sch5 = string.IsNullOrEmpty(m_schooltown) ? "" : m_schooltown;
            sch6 = string.IsNullOrEmpty(m_schoolpostcode) ? "" : m_schoolpostcode;
            sLow = string.IsNullOrEmpty(m_sLow) ? "" : m_sLow;
            sHigh = string.IsNullOrEmpty(m_sHigh) ? "" : m_sHigh;
            schTel = string.IsNullOrEmpty(m_tel) ? "" : m_tel;
            schEmail = string.IsNullOrEmpty(m_email) ? "" : m_email;
            schWeb = string.IsNullOrEmpty(m_web) ? "" : m_web;
            schHead = string.IsNullOrEmpty(m_head) ? "" : m_head;
            schActive = m_active;
            hasYears = m_hasYears;
            hasGroups = m_hasGroups;
            organisationTypeName = m_organisationTypeName;
        }

        public School(string m_res, int m_schguid, string m_schoolName, string m_schoolStreet, string m_schoolLocality, string m_schoolAddress3, string m_schooltown, string m_schoolpostcode, string m_sLow, string m_sHigh, string m_tel, string m_email, string m_web, string m_head, bool m_active, bool m_hasYears, bool m_hasGroups, string m_activeYears, string m_activeGroups, string m_organisationTypeName)
        {
            res = m_res; // please leave this as is. Paul 30/01/2013
            schoopID = m_schguid;
            sch1 = string.IsNullOrEmpty(m_schoolName) ? "" : m_schoolName;
            sch2 = string.IsNullOrEmpty(m_schoolStreet) ? "" : m_schoolStreet;
            sch3 = string.IsNullOrEmpty(m_schoolLocality) ? "" : m_schoolLocality;
            sch4 = string.IsNullOrEmpty(m_schoolAddress3) ? "" : m_schoolAddress3;
            sch5 = string.IsNullOrEmpty(m_schooltown) ? "" : m_schooltown;
            sch6 = string.IsNullOrEmpty(m_schoolpostcode) ? "" : m_schoolpostcode;
            sLow = string.IsNullOrEmpty(m_sLow) ? "" : m_sLow;
            sHigh = string.IsNullOrEmpty(m_sHigh) ? "" : m_sHigh;
            schTel = string.IsNullOrEmpty(m_tel) ? "" : m_tel;
            schEmail = string.IsNullOrEmpty(m_email) ? "" : m_email;
            schWeb = string.IsNullOrEmpty(m_web) ? "" : m_web;
            schHead = string.IsNullOrEmpty(m_head) ? "" : m_head;
            schActive = m_active;
            activeYears = string.IsNullOrEmpty(m_activeYears) ? "" : m_activeYears;
            hasYears = m_hasYears;
            hasGroups = m_hasGroups;
            organisationTypeName = m_organisationTypeName;
            activeGroups = m_activeGroups;
        }
    }

    public class ActiveGroups
    {
        public string res;
        public ActiveGroups()
        {
            groups = new List<Group>();
        }
        public int year;
        public int schoopID;
        public List<Group> groups;

    }

    public class Group
    {
        public int gID;
        public string gName;
        public DateTime gUp;
        public Group(int m_gID, string m_gName, DateTime m_gUp)
        {
            gID = m_gID;
            gName = m_gName;
            gUp = m_gUp;
        }
    }

    public class ActiveGroupsNoYear
    {
        public string res;
        public ActiveGroupsNoYear()
        {
            groups = new List<Group>();
        }
        public int schoopID;
        public List<Group> groups;

    }

    public class ActiveGroupsWithSubGroups
    {
        public ActiveGroupsWithSubGroups()
        {
            parentGroups = new List<ParentGroups>();
        }
        public string res;
        public int schoopID;
        public List<ParentGroups> parentGroups { get; set; }

    }

    public class ParentGroups
    {
        public ParentGroups()
        {
            subGroups = new List<SubGroups>();
        }
        public int gID;
        public string gName;
        public string gDescription;
        public List<SubGroups> subGroups { get; set; }
    }

    public class SubGroups
    {
        public int gID;
        public int parentGID;
        public string gName;
        public string gDescription;

    }

    public class Form
    {
        //Form Title
        public string fTitle;
        //Alert text
        public string fDesc;
        //form url exactly as the current format for form urls
        public string fURL;
    }

    public class SchoolNews
    {
        public int School_news_ID { get; set; }

        public string School_news_date { get; set; }

        public string School_news_headline { get; set; }

        public string School_news_active_years { get; set; }

        public string School_news_active_groups { get; set; }
    }
}
