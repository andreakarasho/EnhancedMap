----------------------------------------------------------------
-- Global Variables
----------------------------------------------------------------

MapWindow = {}

----------------------------------------------------------------
-- Local Variables
----------------------------------------------------------------
			  
MapWindow.Rotation = 45
MapWindow.ZoomScale = 0.1
MapWindow.IsDragging = false
MapWindow.IsMouseOver = false
MapWindow.TypeEnabled = {}
MapWindow.LegendVisible = false
MapWindow.CenterOnPlayer = true

MapWindow.WINDOW_WIDTH_MAX = 716
MapWindow.WINDOW_HEIGHT_MAX = 776
MapWindow.MAP_WIDTH_DIFFERENCE = 26
MapWindow.MAP_HEIGHT_DIFFERENCE = 111

MapWindow.LegendItemTextColors = { normal={r=255,g=255,b=255}, disabled={r=80,g=80,b=80} }
-----------------------------------------------------------------
-- MapCommon Helper Functions
-----------------------------------------------------------------

----------------------------------------------------------------
-- Event Functions
----------------------------------------------------------------

function MapWindow.Initialize()
	WindowUtils.RestoreWindowPosition("MapWindow", true)
	MapWindow.OnResizeEnd("MapWindow")
	
    -- Static text initialization
    WindowUtils.SetWindowTitle("MapWindow",GetStringFromTid(MapCommon.TID.Atlas))

    -- Update registration
    RegisterWindowData(WindowData.Radar.Type,0)
    RegisterWindowData(WindowData.WaypointDisplay.Type,0)
    RegisterWindowData(WindowData.WaypointList.Type,0)
    
    WindowRegisterEventHandler("MapWindow", WindowData.Radar.Event, "MapWindow.UpdateMap")
    WindowRegisterEventHandler("MapWindow", WindowData.WaypointList.Event, "MapWindow.UpdateWaypoints")
    
    local isVisible = WindowGetShowing("MapWindow")
    CreateWindow("LegendWindow",isVisible)
    
    ComboBoxClearMenuItems( "MapWindowFacetCombo" )
    for facet = 0, (MapCommon.NumFacets - 1) do
		Debug.Print("Adding: "..tostring(GetStringFromTid(UORadarGetFacetLabel(facet))))
        ComboBoxAddMenuItem( "MapWindowFacetCombo", GetStringFromTid(UORadarGetFacetLabel(facet)) )
    end
    
    LabelSetText("MapWindowCenterOnPlayerLabel", GetStringFromTid(1112059))
    ButtonSetCheckButtonFlag( "MapWindowCenterOnPlayerButton", true )
    ButtonSetPressedFlag( "MapWindowCenterOnPlayerButton", MapWindow.CenterOnPlayer )
    
    WindowSetScale("MapWindowCoordsText", 0.75 * InterfaceCore.scale)
	WindowSetScale("MapWindowCenterOnPlayerButton", 0.75 * InterfaceCore.scale)
	WindowSetScale("MapWindowCenterOnPlayerLabel", 0.75 * InterfaceCore.scale)
    
    MapWindow.PopulateMapLegend()
end

function MapWindow.Shutdown()
	WindowUtils.SaveWindowPosition("MapWindow")
	
    UnregisterWindowData(WindowData.Radar.Type,0)
    UnregisterWindowData(WindowData.WaypointDisplay.Type,0)
    UnregisterWindowData(WindowData.WaypointList.Type,0)
end

function MapWindow.UpdateMap()
    if( MapCommon.ActiveView == MapCommon.MAP_MODE_NAME ) then
		local facet = UOGetRadarFacet()
		ComboBoxSetSelectedMenuItem( "MapWindowFacetCombo", (facet + 1) )    
        
        ComboBoxClearMenuItems( "MapWindowAreaCombo" )
        for areaIndex = 0, (UORadarGetAreaCount(facet) - 1) do
            ComboBoxAddMenuItem( "MapWindowAreaCombo", GetStringFromTid(UORadarGetAreaLabel(facet, areaIndex)) )
        end
	
        local area = UOGetRadarArea()
        ComboBoxSetSelectedMenuItem( "MapWindowAreaCombo", (area + 1) )

        DynamicImageSetTextureScale("MapImage", WindowData.Radar.TexScale)
        DynamicImageSetTexture("MapImage","radar_texture", WindowData.Radar.TexCoordX, WindowData.Radar.TexCoordY)
        DynamicImageSetRotation("MapImage", WindowData.Radar.TexRotation)
        
        MapCommon.WaypointsDirty = true
    end

    waypointX = WindowData.PlayerLocation.x
    waypointY = WindowData.PlayerLocation.y
    waypointFacet = WindowData.PlayerLocation.facet
    TextLogCreate("pos", 1)
    TextLogSetEnabled("pos", true)
    TextLogClear("pos")
    TextLogSetIncrementalSaving( "pos", true, "logs/pos.log")
    TextLogAddFilterType( "pos", 1, L"XY: " )
    TextLogAddEntry("pos", 1, L" "..waypointFacet..L"|"..waypointX..L"|"..waypointY..L"!")
    TextLogDestroy("pos")

end

function MapWindow.UpdateWaypoints()
    if( MapCommon.ActiveView == MapCommon.MAP_MODE_NAME ) then
        MapCommon.WaypointsDirty = true
    end
end

function MapWindow.PopulateMapLegend()
    if( WindowData.WaypointDisplay.displayTypes.ATLAS ~= nil and WindowData.WaypointDisplay.typeNames ~= nil ) then
        local prevWindowName = nil
    
        for index=1, table.getn(WindowData.WaypointDisplay.typeNames) do
            if WindowData.WaypointDisplay.displayTypes.ATLAS[index].isDisplayed then
                local windowName = "MapLegend"..index             
                
                CreateWindowFromTemplate(windowName,"MapLegendItemTemplate", "LegendWindow" )
                WindowSetId(windowName, index)
                
                if( prevWindowName == nil ) then
                    WindowAddAnchor(windowName, "top", "LegendWindow", "top", 10, 10)
                else
                    WindowAddAnchor(windowName, "bottom", prevWindowName, "top", 0, 0)
                end
                prevWindowName = windowName
                
                local waypointName = WindowData.WaypointDisplay.typeNames[index]
                LabelSetText(windowName.."Text", waypointName)
                
                local iconId = WindowData.WaypointDisplay.displayTypes.ATLAS[index].iconId
                MapCommon.UpdateWaypointIcon(iconId,windowName.."Icon") 
                
                MapWindow.TypeEnabled[index] = true
            end
        end
    end
end

function MapWindow.ActivateMap()
    if( MapCommon.ActiveView ~= MapCommon.MAP_MODE_NAME ) then
        local mapTextureWidth, mapTextureHeight = WindowGetDimensions("MapImage")

	    UORadarSetWindowSize(mapTextureWidth, mapTextureHeight, false, MapWindow.CenterOnPlayer)
	    UOSetRadarRotation(MapWindow.Rotation)
	    UORadarSetWindowOffset(0, 0)

	    WindowSetShowing("RadarWindow", false)
	    WindowSetShowing("MapWindow", true)
	    
	    MapCommon.ActiveView = MapCommon.MAP_MODE_NAME
	    UOSetWaypointDisplayMode(MapCommon.MAP_MODE_NAME)
	    
	    local facet = UOGetRadarFacet()
	    local area = UOGetRadarArea()
	    MapCommon.UpdateZoomValues(facet, area)
	    if(MapWindow.CenterOnPlayer == true) then
			MapCommon.AdjustZoom(-4)
		else
			MapCommon.AdjustZoom(0)
		end
	    
	    MapWindow.UpdateMap()
	    MapWindow.UpdateWaypoints()
	end
end

-----------------------------------------------------------------
-- Input Event Handlers
-----------------------------------------------------------------

function MapWindow.MapOnMouseWheel(x, y, delta)
   	MapCommon.AdjustZoom(-delta)
end

function MapWindow.ZoomOutOnLButtonUp()
   	MapCommon.AdjustZoom(1)
end

function MapWindow.ZoomOutOnMouseOver()
	Tooltips.CreateTextOnlyTooltip(SystemData.ActiveWindow.name, GetStringFromTid(MapCommon.TID.ZoomOut))
	Tooltips.Finalize()
	Tooltips.AnchorTooltip( Tooltips.ANCHOR_WINDOW_TOP )
end

function MapWindow.ZoomInOnLButtonUp()
    MapCommon.AdjustZoom(-1)
end

function MapWindow.ZoomInOnMouseOver()
	Tooltips.CreateTextOnlyTooltip(SystemData.ActiveWindow.name, GetStringFromTid(MapCommon.TID.ZoomIn))
	Tooltips.Finalize()
	Tooltips.AnchorTooltip( Tooltips.ANCHOR_WINDOW_TOP )
end

function MapWindow.MapMouseDrag(flags,deltaX,deltaY)
    if( MapWindow.IsDragging and (deltaX ~= 0 or deltaY ~= 0) ) then
        MapCommon.SetWaypointsEnabled(MapCommon.ActiveView, false)
        
        local facet = UOGetRadarFacet()
        local area = UOGetRadarArea()
        
        local top, bottom, left, right = MapCommon.GetRadarBorders(facet, area)
        
        if ( (deltaX < 0 and right < MapCommon.MapBorder.RIGHT ) or ( deltaX >= 0 and left > MapCommon.MapBorder.LEFT ) ) then
			deltaX = 0
        end
        
        if ( ( deltaY < 0 and bottom < MapCommon.MapBorder.BOTTOM ) or ( deltaY >= 0 and top > MapCommon.MapBorder.TOP ) ) then
			deltaY = 0
        end
        
		local mapCenterX, mapCenterY = UOGetRadarCenter()
		local winCenterX, winCenterY = UOGetWorldPosToRadar(mapCenterX,mapCenterY)
		
		local offsetX = winCenterX - deltaX
		local offsetY = winCenterY - deltaY
		local useScale = false
	       
		local newCenterX, newCenterY = UOGetRadarPosToWorld(offsetX,offsetY,useScale)

		UOCenterRadarOnLocation(newCenterX, newCenterY, facet, area)
	        
		MapCommon.WaypointsDirty = true
    end
end

function MapWindow.ToggleRadarOnLButtonUp()
    RadarWindow.ActivateRadar()
end

function MapWindow.ToggleRadarOnMouseOver()
	Tooltips.CreateTextOnlyTooltip(SystemData.ActiveWindow.name, GetStringFromTid(MapCommon.TID.ShowRadar))
	Tooltips.Finalize()
	Tooltips.AnchorTooltip( Tooltips.ANCHOR_WINDOW_TOP )
end

function MapWindow.ToggleFacetUpOnLButtonUp()
	local facet = UOGetRadarFacet() + 1
	
	if (facet >= MapCommon.NumFacets) then
		facet = 0
	end

	MapWindow.CenterOnPlayer = false
    ButtonSetPressedFlag( "MapWindowCenterOnPlayerButton", MapWindow.CenterOnPlayer )

	MapCommon.ChangeMap(facet, 0)
end

function MapWindow.ToggleFacetDownOnLButtonUp()
	local facet = UOGetRadarFacet() - 1
	
	if (facet < 0) then
		facet = MapCommon.NumFacets - 1
	end
	
	MapWindow.CenterOnPlayer = false
    ButtonSetPressedFlag( "MapWindowCenterOnPlayerButton", MapWindow.CenterOnPlayer )
	
	MapCommon.ChangeMap(facet,0)
end

function MapWindow.ToggleAreaUpOnLButtonUp()
	local facet = UOGetRadarFacet()
	local area = UOGetRadarArea() + 1
	
	if (area >= UORadarGetAreaCount(facet)) then
		area = 0
	end
	
	MapWindow.CenterOnPlayer = false
    ButtonSetPressedFlag( "MapWindowCenterOnPlayerButton", MapWindow.CenterOnPlayer )

	MapCommon.ChangeMap(facet, area)
end

function MapWindow.ToggleAreaDownOnLButtonUp()
	local facet = UOGetRadarFacet()
	local area = UOGetRadarArea() - 1
	
	if (area < 0) then
		area = UORadarGetAreaCount(facet) - 1
	end

	MapWindow.CenterOnPlayer = false
    ButtonSetPressedFlag( "MapWindowCenterOnPlayerButton", MapWindow.CenterOnPlayer )

	MapCommon.ChangeMap(facet, area)
end

function MapWindow.MapOnRButtonUp(flags,x,y)
	local useScale = true
	local waypointX, waypointY = UOGetRadarPosToWorld(x, y, useScale)
	local params = {x=waypointX, y=waypointY, facetId=UOGetRadarFacet()} 
	
	local facet = UOGetRadarFacet()
	local area = UOGetRadarArea()
	local x1, y1, x2, y2 = UORadarGetAreaDimensions(facet, area)
	
	if (x1 < waypointX and y1 < waypointY and x2 > waypointX and y2 > waypointY) then
		ContextMenu.CreateLuaContextMenuItem(MapCommon.TID.CreateWaypoint,0,MapCommon.ContextReturnCodes.CREATE_WAYPOINT,params)
		ContextMenu.ActivateLuaContextMenu(MapCommon.ContextMenuCallback)
	end
end

function MapWindow.LegendIconOnLButtonUp()
    local windowName = SystemData.ActiveWindow.name
    waypointType = WindowGetId(windowName)
    
    MapWindow.TypeEnabled[waypointType] = not MapWindow.TypeEnabled[waypointType]
    
    local alpha = 1.0
    local color = MapWindow.LegendItemTextColors.normal
    if( MapWindow.TypeEnabled[waypointType] == false ) then
		alpha = 0.5
		color = MapWindow.LegendItemTextColors.disabled
	end
    WindowSetAlpha(windowName,alpha)
    LabelSetTextColor(windowName.."Text",color.r,color.g,color.b)
    
    MapCommon.WaypointsDirty = true
end

function MapWindow.CenterOnPlayerOnLButtonUp()
	MapWindow.CenterOnPlayer = ButtonGetPressedFlag( "MapWindowCenterOnPlayerButton" )
	UORadarSetCenterOnPlayer(MapWindow.CenterOnPlayer)
end

function MapWindow.MapOnLButtonDown()
    MapWindow.IsDragging = true
    
    MapWindow.CenterOnPlayer = false
    ButtonSetPressedFlag( "MapWindowCenterOnPlayerButton", MapWindow.CenterOnPlayer )
    UORadarSetCenterOnPlayer(MapWindow.CenterOnPlayer)
end

function MapWindow.MapOnLButtonUp()
    MapWindow.IsDragging = false
    MapCommon.SetWaypointsEnabled(MapCommon.ActiveView, true)
end

function MapWindow.OnMouseOver()
	MapWindow.IsMouseOver = true
end

function MapWindow.OnMouseOverEnd()
    MapWindow.IsDragging = false
    MapWindow.IsMouseOver = false
    MapCommon.SetWaypointsEnabled(MapCommon.ActiveView, true)
end

function MapWindow.SelectArea()
	local facet = UOGetRadarFacet()
    local area = ( ComboBoxGetSelectedMenuItem( "MapWindowAreaCombo" ) - 1 )
    
    if( area ~= UOGetRadarArea() ) then
		MapWindow.CenterOnPlayer = false
        ButtonSetPressedFlag( "MapWindowCenterOnPlayerButton", MapWindow.CenterOnPlayer )
        
        MapCommon.ChangeMap(facet, area )
    end
end

function MapWindow.SelectFacet()
    local facet = ( ComboBoxGetSelectedMenuItem( "MapWindowFacetCombo" ) - 1 )
    local area = UOGetRadarArea()
    
    if( facet ~= UOGetRadarFacet() ) then
		MapWindow.CenterOnPlayer = false
        ButtonSetPressedFlag( "MapWindowCenterOnPlayerButton", MapWindow.CenterOnPlayer )
        
        MapCommon.ChangeMap(facet, 0 )
    end
end

function MapWindow.OnLegendToggle()
	MapWindow.LegendVisible = not MapWindow.LegendVisible
	--Debug.Print("LegendWindow Visible: "..tostring(MapWindow.LegendVisible))
	ButtonSetPressedFlag("MapWindowLegendToggle", MapWindow.LegendVisible)
	WindowSetShowing("LegendWindow",MapWindow.LegendVisible)
end

function MapWindow.OnShown()
	if( MapWindow.LegendVisible == true ) then
		WindowSetShowing("LegendWindow",true)
	end
end

function MapWindow.OnUpdate()
	if( WindowGetShowing("MapWindow") == true and MapWindow.IsMouseOver == true) then
		local windowX, windowY = WindowGetScreenPosition("MapImage")
		local mouseX = SystemData.MousePosition.x - windowX
		local mouseY = SystemData.MousePosition.y - windowY
	    local useScale = true
	    local x, y = UOGetRadarPosToWorld(mouseX, mouseY, useScale)

		local facet = UOGetRadarFacet()
		local area = UOGetRadarArea()	    
	    local x1, y1, x2, y2 = UORadarGetAreaDimensions(facet, area)
		if (x1 < x and y1 < y and x2 > x and y2 > y) then
			local latStr, longStr, latDir, longDir = MapCommon.GetSextantLocationStrings(x, y, facet)
			LabelSetText("MapWindowCoordsText", latStr..L"'"..latDir..L" "..longStr..L"'"..longDir)
		else
			LabelSetText("MapWindowCoordsText", L"")
		end
	elseif (MapCommon.WaypointIsMouseOver == false) then
		LabelSetText("MapWindowCoordsText", L"")
	end
end

function MapWindow.OnHidden()
	WindowSetShowing("LegendWindow",false)
end

function MapWindow.OnLegendButtonMouseOver()
	Tooltips.CreateTextOnlyTooltip(SystemData.ActiveWindow.name, GetStringFromTid(MapCommon.TID.ShowLegend))
	Tooltips.Finalize()
	Tooltips.AnchorTooltip( Tooltips.ANCHOR_WINDOW_TOP )
end

function MapWindow.OnResizeBegin()
	local windowName = WindowUtils.GetActiveDialog()
	local widthMin = 400
	local heightMin = 400
    WindowUtils.BeginResize( windowName, "topleft", widthMin, heightMin, false, MapWindow.OnResizeEnd)
end

function MapWindow.OnResizeEnd(curWindow)
	local windowWidth, windowHeight = WindowGetDimensions("MapWindow")
	
	if(windowWidth > MapWindow.WINDOW_WIDTH_MAX) then
		windowWidth = MapWindow.WINDOW_WIDTH_MAX
	end
	
	if(windowHeight > MapWindow.WINDOW_HEIGHT_MAX) then
		windowHeight = MapWindow.WINDOW_HEIGHT_MAX
	end
	
	local legendScale = windowHeight / MapWindow.WINDOW_HEIGHT_MAX
	WindowSetScale("LegendWindow", legendScale * InterfaceCore.scale)
	
	WindowSetDimensions("MapWindow", windowWidth, windowHeight)
	WindowSetDimensions("Map", windowWidth - MapWindow.MAP_WIDTH_DIFFERENCE, windowHeight - MapWindow.MAP_HEIGHT_DIFFERENCE)
end