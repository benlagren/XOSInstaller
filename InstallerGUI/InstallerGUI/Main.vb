﻿Imports System.Drawing.Text
Imports System.IO
Imports xdevs23.Localization
Imports xdui

Public Class Main

    Private LangManager As LanguageManager
    Private RobotoCondensed, RobotoLight, RobotoThin As PrivateFontCollection
    Private CurrentPage As Integer = 0

    Private Shared ReadOnly _
            STRING_EXIT_CONFIRMATION As String = "confirm_exit",
            STRING_EXIT_CONF_TITLE   As String = "confirm_exit_title"

    Private Sub BtnCancel_Click(sender As Object, e As EventArgs) Handles BtnCancel.Click
        If MessageBox.Show(LangManager.GetString(STRING_EXIT_CONFIRMATION),
                           LangManager.GetString(STRING_EXIT_CONF_TITLE),
                           MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Close()
            End
        End If
    End Sub

    Private Sub HandlePage(PageName As String)
        Select Case PageName
            Case PageDetectDevicecPanel.Name
                Dim bla As New Threading.Thread( _
                    New Threading.ThreadStart(AddressOf DoAdbDetect) _
                )
                bla.Start()
        End Select
    End Sub

    Private Sub ChangePage(PageNum As Integer)
        Dim PageCount As Integer = 0
        For Each C As Control In Controls
            If isNothing(C.Tag) OrElse C.Tag.ToString().Equals("") Then Continue For
            If TypeOf(C) Is Panel AndAlso C.Name.Contains("Page")
                If Integer.Parse(C.Tag.ToString()) = CurrentPage
                    C.SendToBack()
                    C.Visible = False
                End If
                PageCount += 1
                If Integer.Parse(C.Tag.ToString()) = PageNum Then
                    C.Visible = True
                    C.BringToFront()
                    CurrentPage = Integer.Parse(C.Tag.ToString())
                    BtnBack.Visible = (CurrentPage > 0)
                    HandlePage(C.Name)
                End If
            End If
        Next
        Debug.WriteLine(CurrentPage)
        Debug.WriteLine(PageCount)
        BtnNext.Visible = Not (CurrentPage = PageCount - 1)
    End Sub

    Private Sub DoAdbDetect()
        Debug.WriteLine("Detecting device...")
        AdbHelper.ExecuteAdbCommand("wait-for-device")
    End Sub

    Private Sub ChangePage(Forward As Boolean)
        ChangePage(CType(IIf(Forward, CurrentPage + 1, CurrentPage - 1), Integer))
    End Sub

    Private Sub NextBackButton_Click(sender As Object, e As EventArgs) Handles BtnNext.Click, BtnBack.Click
        ChangePage(CType(sender, FlatButton).Equals(BtnNext))
    End Sub

    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Load fonts
        RobotoCondensed = New PrivateFontCollection
        RobotoLight = New PrivateFontCollection
        RobotoThin = New PrivateFontCollection
        For Each FontFile As String
                In Directory.GetFiles("fonts/", "Roboto-*.ttf", SearchOption.AllDirectories)
            Debug.WriteLine("Debug: Loading font file " & FontFile & " for main window")
            If FontFile.Contains("Thin") Then
                RobotoThin.AddFontFile(FontFile)
            ElseIf FontFile.Contains("Light") Then
                RobotoLight.AddFontFile(FontFile)
            End If
        Next
        For Each FontFile As String
                In Directory.GetFiles("fonts/", "RobotoCondensed-*.ttf", SearchOption.AllDirectories)
            RobotoCondensed.AddFontFile(FontFile)
        Next
        Font = New Font(RobotoCondensed.Families(0), 9.25, FontStyle.Regular)

        ' Load languages
        LangManager = New LanguageManager()
        LangManager.AutoApplyLanguage(Me)

        ' Prepare the form
        Try
            Icon = New Icon("xosicon.ico")
        Catch ex As ComponentModel.Win32Exception
            Dim lineToWrite As String = "Warning: Failed to load icon, assuming linux, skipping this step."
            Debug.WriteLine(lineToWrite)
            Console.WriteLine(lineToWrite)
        End Try
 
        Size = New Size(820, 640)
        For Each C As Control In Controls
            If TypeOf (C) Is Panel AndAlso C.Name.Contains("Page") Then
                C.Size = New Size(800, 420) ' Have the same size for all panels
                C.Location = New Point(2, 94)
            End If
            If TypeOf (C) Is FlatButton Then
                C.Font = Font
            End If
        Next

        ' Prepare the first panel
        ChangePage(CurrentPage)
    End Sub

End Class
