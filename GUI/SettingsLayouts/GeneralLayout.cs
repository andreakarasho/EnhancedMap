using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using EnhancedMap.Core;

namespace EnhancedMap.GUI.SettingsLayouts
{
    public partial class GeneralLayout : UserControl
    {
        public GeneralLayout()
        {
            InitializeComponent();

            /*UserObject obj = new UserObject("User");


            pictureBoxMapEx.Paint += (sender, e) =>
            {
                Graphics g = e.Graphics;

                obj.Render(g, pictureBoxMapEx.Width / 2, pictureBoxMapEx.Height / 2, pictureBoxMapEx.Width, pictureBoxMapEx.Height);
            };*/
        }


        public void LoadXML(XmlElement root)
        {
            Global.SettingsCollection["showhits"] = checkBoxShowHits.Checked = root["general"].ToText("showhits", Global.SettingsCollection["showhits"].ToString()).ToBool();
            Global.SettingsCollection["showstamina"] = checkBoxShowStamina.Checked = root["general"].ToText("showstamina", Global.SettingsCollection["showstamina"].ToString()).ToBool();
            Global.SettingsCollection["showmana"] = checkBoxShowMana.Checked = root["general"].ToText("showmana", Global.SettingsCollection["showmana"].ToString()).ToBool();
            Global.SettingsCollection["centerplayer"] = checkBoxCenterPlayer.Checked = root["general"].ToText("centerplayer", Global.SettingsCollection["centerplayer"].ToString()).ToBool();
            Global.SettingsCollection["trackdeathpoint"] = checkBoxTrackDeathPoint.Checked = root["general"].ToText("trackdeathpoint", Global.SettingsCollection["trackdeathpoint"].ToString()).ToBool();
            Global.SettingsCollection["showhiddenicon"] = checkBoxShowHiddenIcon.Checked = root["general"].ToText("showhiddenicon", Global.SettingsCollection["showhiddenicon"].ToString()).ToBool();
            Global.SettingsCollection["showdeathicon"] = checkBoxShowDeathIcon.Checked = root["general"].ToText("showdeathicon", Global.SettingsCollection["showdeathicon"].ToString()).ToBool();
            Global.SettingsCollection["abbreviatenames"] = checkBoxAbbreviateNames.Checked = root["general"].ToText("abbreviatenames", Global.SettingsCollection["abbreviatenames"].ToString()).ToBool();
            Global.SettingsCollection["panicsounds"] = checkBoxPanicSounds.Checked = root["general"].ToText("panicsounds", Global.SettingsCollection["panicsounds"].ToString()).ToBool();
            Global.SettingsCollection["alertsounds"] = checkBoxAlertSounds.Checked = root["general"].ToText("alertsounds", Global.SettingsCollection["alertsounds"].ToString()).ToBool();
            Global.SettingsCollection["smartnamesposition"] = checkBoxSmartNamePosition.Checked = root["general"].ToText("smartnamesposition", Global.SettingsCollection["smartnamesposition"].ToString()).ToBool();
            Global.SettingsCollection["showplacesicons"] = checkBoxShowPlacesIcons.Checked = root["general"].ToText("showplacesicons", Global.SettingsCollection["showplacesicons"].ToString()).ToBool();
            Global.SettingsCollection["hidelessimportantplaces"] = checkBoxHideLessImportantPlaces.Checked = root["general"].ToText("hidelessimportantplaces", Global.SettingsCollection["hidelessimportantplaces"].ToString()).ToBool();
            Global.SettingsCollection["showtownsnames"] = checkBoxShowTownsNames.Checked = root["general"].ToText("showtownsnames", Global.SettingsCollection["showtownsnames"].ToString()).ToBool();
            Global.SettingsCollection["showserverbounds"] = checkBoxShowServerBounds.Checked = root["general"].ToText("showserverbounds", Global.SettingsCollection["showserverbounds"].ToString()).ToBool();
            Global.SettingsCollection["showguardlines"] = checkBoxShowGuardlines.Checked = root["general"].ToText("showguardlines", Global.SettingsCollection["showguardlines"].ToString()).ToBool();
            Global.SettingsCollection["showcoordinates"] = checkBoxShowCoordinates.Checked = root["general"].ToText("showcoordinates", Global.SettingsCollection["showcoordinates"].ToString()).ToBool();
            Global.SettingsCollection["showhouses"] = checkBoxShowHouses.Checked = root["general"].ToText("showhouses", Global.SettingsCollection["showhouses"].ToString()).ToBool();
            Global.SettingsCollection["showmobilesaroundyou"] = checkBoxShowMobilesAround.Checked = root["general"].ToText("showmobilesaroundyou", Global.SettingsCollection["showmobilesaroundyou"].ToString()).ToBool();
            Global.SettingsCollection["dontrenderusersindifferentfacets"] = checkBoxDontRenderUsersDiffFacet.Checked = root["general"].ToText("dontrenderusersindifferentfacets", Global.SettingsCollection["dontrenderusersindifferentfacets"].ToString()).ToBool();
            Global.SettingsCollection["nameposition"] = trackBarNamePosition.Value = root["general"].ToText("nameposition", Global.SettingsCollection["nameposition"].ToString()).ToInt();

            Global.SettingsCollection["namefont"] = root["general"].ToText("namefont", Global.SettingsCollection["namefont"].ToString());
            Global.SettingsCollection["namecolor"] = root["general"].ToText("namecolor", Global.SettingsCollection["namecolor"].ToString()).ToInt();
            Global.SettingsCollection["namesize"] = root["general"].ToText("namesize", Global.SettingsCollection["namesize"].ToString()).ToInt();
            Global.SettingsCollection["namestyle"] = root["general"].ToText("namestyle", Global.SettingsCollection["namestyle"].ToString()).ToInt();

            AssignEvents();
        }

        public void SaveXML(XmlWriter writer)
        {
            writer.WriteStartElement("general");

            writer.WriteAttributeString("showhits", checkBoxShowHits.Checked.ToString());
            writer.WriteAttributeString("showstamina", checkBoxShowStamina.Checked.ToString());
            writer.WriteAttributeString("showmana", checkBoxShowMana.Checked.ToString());
            writer.WriteAttributeString("centerplayer", checkBoxCenterPlayer.Checked.ToString());
            writer.WriteAttributeString("trackdeathpoint", checkBoxTrackDeathPoint.Checked.ToString());
            writer.WriteAttributeString("showhiddenicon", checkBoxShowHiddenIcon.Checked.ToString());
            writer.WriteAttributeString("showdeathicon", checkBoxShowDeathIcon.Checked.ToString());
            writer.WriteAttributeString("abbreviatenames", checkBoxAbbreviateNames.Checked.ToString());
            writer.WriteAttributeString("panicsounds", checkBoxPanicSounds.Checked.ToString());
            writer.WriteAttributeString("alertsounds", checkBoxAlertSounds.Checked.ToString());
            writer.WriteAttributeString("smartnamesposition", checkBoxSmartNamePosition.Checked.ToString());
            writer.WriteAttributeString("showplacesicons", checkBoxShowPlacesIcons.Checked.ToString());
            writer.WriteAttributeString("hidelessimportantplaces", checkBoxHideLessImportantPlaces.Checked.ToString());
            writer.WriteAttributeString("showtownsnames", checkBoxShowTownsNames.Checked.ToString());
            writer.WriteAttributeString("showserverbounds", checkBoxShowServerBounds.Checked.ToString());
            writer.WriteAttributeString("showguardlines", checkBoxShowGuardlines.Checked.ToString());
            writer.WriteAttributeString("showcoordinates", checkBoxShowCoordinates.Checked.ToString());
            writer.WriteAttributeString("showhouses", checkBoxShowHouses.Checked.ToString());
            writer.WriteAttributeString("showmobilesaroundyou", checkBoxShowMobilesAround.Checked.ToString());
            writer.WriteAttributeString("dontrenderusersindifferentfacets", checkBoxDontRenderUsersDiffFacet.Checked.ToString());
            writer.WriteAttributeString("nameposition", trackBarNamePosition.Value.ToString());

            writer.WriteAttributeString("namecolor", Global.SettingsCollection["namecolor"].ToString());
            writer.WriteAttributeString("namefont", Global.SettingsCollection["namefont"].ToString());
            writer.WriteAttributeString("namesize", Global.SettingsCollection["namesize"].ToString());
            writer.WriteAttributeString("namestyle", Global.SettingsCollection["namestyle"].ToString());

            writer.WriteEndElement();


            Global.SettingsCollection["showhits"] = checkBoxShowHits.Checked;
            Global.SettingsCollection["showstamina"] = checkBoxShowStamina.Checked;
            Global.SettingsCollection["showmana"] = checkBoxShowMana.Checked;
            Global.SettingsCollection["centerplayer"] = checkBoxCenterPlayer.Checked;
            Global.SettingsCollection["trackdeathpoint"] = checkBoxTrackDeathPoint.Checked;
            Global.SettingsCollection["showhiddenicon"] = checkBoxShowHiddenIcon.Checked;
            Global.SettingsCollection["showdeathicon"] = checkBoxShowDeathIcon.Checked;
            Global.SettingsCollection["abbreviatenames"] = checkBoxAbbreviateNames.Checked;
            Global.SettingsCollection["panicsounds"] = checkBoxPanicSounds.Checked;
            Global.SettingsCollection["alertsounds"] = checkBoxAlertSounds.Checked;
            Global.SettingsCollection["smartnamesposition"] = checkBoxSmartNamePosition.Checked;
            Global.SettingsCollection["showplacesicons"] = checkBoxShowPlacesIcons.Checked;
            Global.SettingsCollection["hidelessimportantplaces"] = checkBoxHideLessImportantPlaces.Checked;
            Global.SettingsCollection["showtownsnames"] = checkBoxShowTownsNames.Checked;
            Global.SettingsCollection["showserverbounds"] = checkBoxShowServerBounds.Checked;
            Global.SettingsCollection["showguardlines"] = checkBoxShowGuardlines.Checked;
            Global.SettingsCollection["showcoordinates"] = checkBoxShowCoordinates.Checked;
            Global.SettingsCollection["showhouses"] = checkBoxShowHouses.Checked;
            Global.SettingsCollection["showmobilesaroundyou"] = checkBoxShowMobilesAround.Checked;
            Global.SettingsCollection["dontrenderusersindifferentfacets"] = checkBoxDontRenderUsersDiffFacet.Checked;
            Global.SettingsCollection["nameposition"] = trackBarNamePosition.Value;
            /*Global.SettingsCollection["namefont"] = Global.PlayerInstance.Font.Name;
            Global.SettingsCollection["namecolor"] = Global.PlayerInstance.Hue.Color.ToArgb().ToString();
            Global.SettingsCollection["namesize"] = ((int)Global.PlayerInstance.Font.Size).ToString();
            Global.SettingsCollection["namestyle"] = ((int)Global.PlayerInstance.Font.Style).ToString();*/
        }

        public void LoadDefault()
        {
            checkBoxShowHits.Checked = Global.SettingsCollection["showhits"].ToBool();
            checkBoxShowStamina.Checked = Global.SettingsCollection["showstamina"].ToBool();
            checkBoxShowMana.Checked = Global.SettingsCollection["showmana"].ToBool();
            checkBoxCenterPlayer.Checked = Global.SettingsCollection["centerplayer"].ToBool();
            checkBoxTrackDeathPoint.Checked = Global.SettingsCollection["trackdeathpoint"].ToBool();
            checkBoxShowHiddenIcon.Checked = Global.SettingsCollection["showhiddenicon"].ToBool();
            checkBoxShowDeathIcon.Checked = Global.SettingsCollection["showdeathicon"].ToBool();
            checkBoxAbbreviateNames.Checked = Global.SettingsCollection["abbreviatenames"].ToBool();
            checkBoxPanicSounds.Checked = Global.SettingsCollection["panicsounds"].ToBool();
            checkBoxAlertSounds.Checked = Global.SettingsCollection["alertsounds"].ToBool();
            checkBoxSmartNamePosition.Checked = Global.SettingsCollection["smartnamesposition"].ToBool();
            checkBoxShowPlacesIcons.Checked = Global.SettingsCollection["showplacesicons"].ToBool();
            checkBoxHideLessImportantPlaces.Checked = Global.SettingsCollection["hidelessimportantplaces"].ToBool();
            checkBoxShowTownsNames.Checked = Global.SettingsCollection["showtownsnames"].ToBool();
            checkBoxShowServerBounds.Checked = Global.SettingsCollection["showserverbounds"].ToBool();
            checkBoxShowGuardlines.Checked = Global.SettingsCollection["showguardlines"].ToBool();
            checkBoxShowCoordinates.Checked = Global.SettingsCollection["showcoordinates"].ToBool();
            checkBoxShowHouses.Checked = Global.SettingsCollection["showhouses"].ToBool();
            checkBoxShowMobilesAround.Checked = Global.SettingsCollection["showmobilesaroundyou"].ToBool();
            checkBoxDontRenderUsersDiffFacet.Checked = Global.SettingsCollection["dontrenderusersindifferentfacets"].ToBool();
            trackBarNamePosition.Value = Global.SettingsCollection["nameposition"].ToInt();

            AssignEvents();
        }

        private void AssignEvents()
        {
            customButtonSetNameFont.Click += (sender, e) =>
            {
                FontDialog dialog = new FontDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Global.SettingsCollection["namefont"] = dialog.Font.Name;
                    Global.SettingsCollection["namesize"] = (int) dialog.Font.Size;
                    Global.SettingsCollection["namestyle"] = (int) dialog.Font.Style;

                    Font font = Global.PlayerInstance.Font;
                    Global.PlayerInstance.Font = new Font(dialog.Font.Name, dialog.Font.Size < 12 ? 12 : (int) dialog.Font.Size, dialog.Font.Style, GraphicsUnit.Pixel);
                }
            };
            customButtonSetNameHue.Click += (sender, e) =>
            {
                ColorDialog dialog = new ColorDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Global.SettingsCollection["namecolor"] = dialog.Color.ToArgb();
                    Global.PlayerInstance.Hue = new SolidBrush(dialog.Color);
                }
            };
        }
    }
}