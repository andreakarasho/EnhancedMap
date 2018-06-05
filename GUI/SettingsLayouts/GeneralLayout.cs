using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using EnhancedMap.Core;
using EnhancedMap.Core.MapObjects;

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
            Global.SettingsCollection["showhits"] = this.checkBoxShowHits.Checked = root["general"].ToText("showhits", Global.SettingsCollection["showhits"].ToString()).ToBool();
            Global.SettingsCollection["showstamina"] = this.checkBoxShowStamina.Checked = root["general"].ToText("showstamina", Global.SettingsCollection["showstamina"].ToString()).ToBool();
            Global.SettingsCollection["showmana"] = this.checkBoxShowMana.Checked = root["general"].ToText("showmana", Global.SettingsCollection["showmana"].ToString()).ToBool();
            Global.SettingsCollection["centerplayer"] = this.checkBoxCenterPlayer.Checked = root["general"].ToText("centerplayer", Global.SettingsCollection["centerplayer"].ToString()).ToBool();
            Global.SettingsCollection["trackdeathpoint"] = this.checkBoxTrackDeathPoint.Checked = root["general"].ToText("trackdeathpoint", Global.SettingsCollection["trackdeathpoint"].ToString()).ToBool();
            Global.SettingsCollection["showhiddenicon"] = this.checkBoxShowHiddenIcon.Checked = root["general"].ToText("showhiddenicon", Global.SettingsCollection["showhiddenicon"].ToString()).ToBool();
            Global.SettingsCollection["showdeathicon"] = this.checkBoxShowDeathIcon.Checked = root["general"].ToText("showdeathicon", Global.SettingsCollection["showdeathicon"].ToString()).ToBool();
            Global.SettingsCollection["abbreviatenames"] = this.checkBoxAbbreviateNames.Checked = root["general"].ToText("abbreviatenames", Global.SettingsCollection["abbreviatenames"].ToString()).ToBool();
            Global.SettingsCollection["panicsounds"] = this.checkBoxPanicSounds.Checked = root["general"].ToText("panicsounds", Global.SettingsCollection["panicsounds"].ToString()).ToBool();
            Global.SettingsCollection["alertsounds"] = this.checkBoxAlertSounds.Checked = root["general"].ToText("alertsounds", Global.SettingsCollection["alertsounds"].ToString()).ToBool();
            Global.SettingsCollection["smartnamesposition"] = this.checkBoxSmartNamePosition.Checked = root["general"].ToText("smartnamesposition", Global.SettingsCollection["smartnamesposition"].ToString()).ToBool();
            Global.SettingsCollection["showplacesicons"] = this.checkBoxShowPlacesIcons.Checked = root["general"].ToText("showplacesicons", Global.SettingsCollection["showplacesicons"].ToString()).ToBool();
            Global.SettingsCollection["hidelessimportantplaces"] = this.checkBoxHideLessImportantPlaces.Checked = root["general"].ToText("hidelessimportantplaces", Global.SettingsCollection["hidelessimportantplaces"].ToString()).ToBool();
            Global.SettingsCollection["showtownsnames"] = this.checkBoxShowTownsNames.Checked = root["general"].ToText("showtownsnames", Global.SettingsCollection["showtownsnames"].ToString()).ToBool();
            Global.SettingsCollection["showserverbounds"] = this.checkBoxShowServerBounds.Checked = root["general"].ToText("showserverbounds", Global.SettingsCollection["showserverbounds"].ToString()).ToBool();
            Global.SettingsCollection["showguardlines"] = this.checkBoxShowGuardlines.Checked = root["general"].ToText("showguardlines", Global.SettingsCollection["showguardlines"].ToString()).ToBool();
            Global.SettingsCollection["showcoordinates"] = this.checkBoxShowCoordinates.Checked = root["general"].ToText("showcoordinates", Global.SettingsCollection["showcoordinates"].ToString()).ToBool();
            Global.SettingsCollection["showhouses"] = this.checkBoxShowHouses.Checked = root["general"].ToText("showhouses", Global.SettingsCollection["showhouses"].ToString()).ToBool();
            Global.SettingsCollection["showmobilesaroundyou"] = this.checkBoxShowMobilesAround.Checked = root["general"].ToText("showmobilesaroundyou", Global.SettingsCollection["showmobilesaroundyou"].ToString()).ToBool();
            Global.SettingsCollection["dontrenderusersindifferentfacets"] = this.checkBoxDontRenderUsersDiffFacet.Checked = root["general"].ToText("dontrenderusersindifferentfacets", Global.SettingsCollection["dontrenderusersindifferentfacets"].ToString()).ToBool();
            Global.SettingsCollection["nameposition"] = this.trackBarNamePosition.Value = root["general"].ToText("nameposition", Global.SettingsCollection["nameposition"].ToString()).ToInt();

            Global.SettingsCollection["namefont"] = root["general"].ToText("namefont", Global.SettingsCollection["namefont"].ToString());
            Global.SettingsCollection["namecolor"] = root["general"].ToText("namecolor", Global.SettingsCollection["namecolor"].ToString()).ToInt();
            Global.SettingsCollection["namesize"] = root["general"].ToText("namesize", Global.SettingsCollection["namesize"].ToString()).ToInt();
            Global.SettingsCollection["namestyle"] = root["general"].ToText("namestyle", Global.SettingsCollection["namestyle"].ToString()).ToInt();
       
            AssignEvents();
        }

        public void SaveXML(XmlWriter writer)
        {
            writer.WriteStartElement("general");

            writer.WriteAttributeString("showhits", this.checkBoxShowHits.Checked.ToString());
            writer.WriteAttributeString("showstamina", this.checkBoxShowStamina.Checked.ToString());
            writer.WriteAttributeString("showmana", this.checkBoxShowMana.Checked.ToString());
            writer.WriteAttributeString("centerplayer", this.checkBoxCenterPlayer.Checked.ToString());
            writer.WriteAttributeString("trackdeathpoint", this.checkBoxTrackDeathPoint.Checked.ToString());
            writer.WriteAttributeString("showhiddenicon", this.checkBoxShowHiddenIcon.Checked.ToString());
            writer.WriteAttributeString("showdeathicon", this.checkBoxShowDeathIcon.Checked.ToString());
            writer.WriteAttributeString("abbreviatenames", this.checkBoxAbbreviateNames.Checked.ToString());
            writer.WriteAttributeString("panicsounds", this.checkBoxPanicSounds.Checked.ToString());
            writer.WriteAttributeString("alertsounds", this.checkBoxAlertSounds.Checked.ToString());
            writer.WriteAttributeString("smartnamesposition", this.checkBoxSmartNamePosition    .Checked.ToString());
            writer.WriteAttributeString("showplacesicons", this.checkBoxShowPlacesIcons.Checked.ToString());
            writer.WriteAttributeString("hidelessimportantplaces", this.checkBoxHideLessImportantPlaces.Checked.ToString()); 
            writer.WriteAttributeString("showtownsnames", this.checkBoxShowTownsNames.Checked.ToString());
            writer.WriteAttributeString("showserverbounds", this.checkBoxShowServerBounds.Checked.ToString());
            writer.WriteAttributeString("showguardlines", this.checkBoxShowGuardlines.Checked.ToString());
            writer.WriteAttributeString("showcoordinates", this.checkBoxShowCoordinates.Checked.ToString());
            writer.WriteAttributeString("showhouses", this.checkBoxShowHouses.Checked.ToString());
            writer.WriteAttributeString("showmobilesaroundyou", this.checkBoxShowMobilesAround.Checked.ToString());
            writer.WriteAttributeString("dontrenderusersindifferentfacets", this.checkBoxDontRenderUsersDiffFacet.Checked.ToString());
            writer.WriteAttributeString("nameposition", this.trackBarNamePosition.Value.ToString());

            writer.WriteAttributeString("namecolor", Global.SettingsCollection["namecolor"].ToString());
            writer.WriteAttributeString("namefont", Global.SettingsCollection["namefont"].ToString());
            writer.WriteAttributeString("namesize", Global.SettingsCollection["namesize"].ToString());
            writer.WriteAttributeString("namestyle", Global.SettingsCollection["namestyle"].ToString());

            writer.WriteEndElement();


            Global.SettingsCollection["showhits"] = this.checkBoxShowHits.Checked;
            Global.SettingsCollection["showstamina"] = this.checkBoxShowStamina.Checked;
            Global.SettingsCollection["showmana"] = this.checkBoxShowMana.Checked;
            Global.SettingsCollection["centerplayer"] = this.checkBoxCenterPlayer.Checked;
            Global.SettingsCollection["trackdeathpoint"] = this.checkBoxTrackDeathPoint.Checked;
            Global.SettingsCollection["showhiddenicon"] = this.checkBoxShowHiddenIcon.Checked;
            Global.SettingsCollection["showdeathicon"] = this.checkBoxShowDeathIcon.Checked;
            Global.SettingsCollection["abbreviatenames"] = this.checkBoxAbbreviateNames.Checked;
            Global.SettingsCollection["panicsounds"] = this.checkBoxPanicSounds.Checked;
            Global.SettingsCollection["alertsounds"] = this.checkBoxAlertSounds.Checked;
            Global.SettingsCollection["smartnamesposition"] = this.checkBoxSmartNamePosition.Checked;
            Global.SettingsCollection["showplacesicons"] = this.checkBoxShowPlacesIcons.Checked;
            Global.SettingsCollection["hidelessimportantplaces"] = this.checkBoxHideLessImportantPlaces.Checked;
            Global.SettingsCollection["showtownsnames"] = this.checkBoxShowTownsNames.Checked;
            Global.SettingsCollection["showserverbounds"] = this.checkBoxShowServerBounds.Checked;
            Global.SettingsCollection["showguardlines"] = this.checkBoxShowGuardlines.Checked;
            Global.SettingsCollection["showcoordinates"] = this.checkBoxShowCoordinates.Checked;
            Global.SettingsCollection["showhouses"] = this.checkBoxShowHouses.Checked;
            Global.SettingsCollection["showmobilesaroundyou"] = this.checkBoxShowMobilesAround.Checked;
            Global.SettingsCollection["dontrenderusersindifferentfacets"] = this.checkBoxDontRenderUsersDiffFacet.Checked;
            Global.SettingsCollection["nameposition"] = this.trackBarNamePosition.Value;
            /*Global.SettingsCollection["namefont"] = Global.PlayerInstance.Font.Name;
            Global.SettingsCollection["namecolor"] = Global.PlayerInstance.Hue.Color.ToArgb().ToString();
            Global.SettingsCollection["namesize"] = ((int)Global.PlayerInstance.Font.Size).ToString();
            Global.SettingsCollection["namestyle"] = ((int)Global.PlayerInstance.Font.Style).ToString();*/


           
        }

        public void LoadDefault()
        {
            this.checkBoxShowHits.Checked = Global.SettingsCollection["showhits"].ToBool();
            this.checkBoxShowStamina.Checked = Global.SettingsCollection["showstamina"].ToBool();
            this.checkBoxShowMana.Checked = Global.SettingsCollection["showmana"].ToBool();
            this.checkBoxCenterPlayer.Checked = Global.SettingsCollection["centerplayer"].ToBool();
            this.checkBoxTrackDeathPoint.Checked = Global.SettingsCollection["trackdeathpoint"].ToBool();
            this.checkBoxShowHiddenIcon.Checked = Global.SettingsCollection["showhiddenicon"].ToBool();
            this.checkBoxShowDeathIcon.Checked = Global.SettingsCollection["showdeathicon"].ToBool();
            this.checkBoxAbbreviateNames.Checked = Global.SettingsCollection["abbreviatenames"].ToBool();
            this.checkBoxPanicSounds.Checked = Global.SettingsCollection["panicsounds"].ToBool();
            this.checkBoxAlertSounds.Checked = Global.SettingsCollection["alertsounds"].ToBool();
            this.checkBoxSmartNamePosition.Checked = Global.SettingsCollection["smartnamesposition"].ToBool();
            this.checkBoxShowPlacesIcons.Checked = Global.SettingsCollection["showplacesicons"].ToBool();
            this.checkBoxHideLessImportantPlaces.Checked = Global.SettingsCollection["hidelessimportantplaces"].ToBool();
            this.checkBoxShowTownsNames.Checked = Global.SettingsCollection["showtownsnames"].ToBool();
            this.checkBoxShowServerBounds.Checked = Global.SettingsCollection["showserverbounds"].ToBool();
            this.checkBoxShowGuardlines.Checked = Global.SettingsCollection["showguardlines"].ToBool();
            this.checkBoxShowCoordinates.Checked = Global.SettingsCollection["showcoordinates"].ToBool();
            this.checkBoxShowHouses.Checked = Global.SettingsCollection["showhouses"].ToBool();
            this.checkBoxShowMobilesAround.Checked = Global.SettingsCollection["showmobilesaroundyou"].ToBool();
            this.checkBoxDontRenderUsersDiffFacet.Checked =  Global.SettingsCollection["dontrenderusersindifferentfacets"].ToBool();
            this.trackBarNamePosition.Value = Global.SettingsCollection["nameposition"].ToInt();

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
                    Global.SettingsCollection["namesize"] = (int)dialog.Font.Size;
                    Global.SettingsCollection["namestyle"] = (int)dialog.Font.Style;

                    Font font = Global.PlayerInstance.Font;
                    Global.PlayerInstance.Font = new Font(dialog.Font.Name, dialog.Font.Size < 12 ? 12 : (int)dialog.Font.Size, dialog.Font.Style, GraphicsUnit.Pixel);
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
