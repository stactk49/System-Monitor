Imports System.Collections.ObjectModel
Imports System.Management
Imports Microsoft.Win32

Friend Class RegistryWatcher
    Inherits ManagementEventWatcher
    Implements IDisposable

    Private Shared _supportedHives As ReadOnlyCollection(Of RegistryKey) = Nothing


    Public Shared ReadOnly Property SupportedHives() As ReadOnlyCollection(Of RegistryKey)
        Get
            If _supportedHives Is Nothing Then
                Dim hives() As RegistryKey =
                    {
                        Registry.LocalMachine,
                        Registry.Users,
                        Registry.CurrentConfig
                    }
                _supportedHives = Array.AsReadOnly(Of RegistryKey)(hives)
            End If
            Return _supportedHives
        End Get
    End Property


    Private _hive As RegistryKey
    Public Property Hive() As RegistryKey
        Get
            Return _hive
        End Get
        Private Set(ByVal value As RegistryKey)
            _hive = value
        End Set
    End Property

    Private _keyPath As String
    Public Property KeyPath() As String
        Get
            Return _keyPath
        End Get
        Private Set(ByVal value As String)
            _keyPath = value
        End Set
    End Property

    Private _keyToMonitor As RegistryKey
    Public Property KeyToMonitor() As RegistryKey
        Get
            Return _keyToMonitor
        End Get
        Private Set(ByVal value As RegistryKey)
            _keyToMonitor = value
        End Set
    End Property

    Public Event RegistryKeyChangeEvent As EventHandler(Of RegistryKeyChangeEventArgs)


    Public Sub New(ByVal hive As RegistryKey, ByVal keyPath As String)
        Me.Hive = hive
        Me.KeyPath = keyPath

        Me.KeyToMonitor = hive.OpenSubKey(keyPath)

        If KeyToMonitor IsNot Nothing Then
            Dim queryString As String = String.Format("SELECT * FROM RegistryKeyChangeEvent " & ControlChars.CrLf & "                   WHERE Hive = '{0}' AND KeyPath = '{1}' ", Me.Hive.Name, Me.KeyPath)

            Dim query As New WqlEventQuery()
            query.QueryString = queryString
            query.EventClassName = "RegistryKeyChangeEvent"
            query.WithinInterval = New TimeSpan(0, 0, 0, 1)
            Me.Query = query

            AddHandler EventArrived, AddressOf RegistryWatcher_EventArrived
        Else
            Dim message As String = String.Format("The registry key {0}\{1} does not exist", hive.Name, keyPath)
            Throw New ArgumentException(message)

        End If
    End Sub

    Private Sub RegistryWatcher_EventArrived(ByVal sender As Object, ByVal e As EventArrivedEventArgs)

        Dim args As New RegistryKeyChangeEventArgs(e.NewEvent)

        RaiseEvent RegistryKeyChangeEvent(sender, args)

    End Sub

    Public Shadows Sub Dispose() Implements IDisposable.Dispose
        MyBase.Dispose()
        If Me.KeyToMonitor IsNot Nothing Then
            Me.KeyToMonitor.Dispose()
        End If
    End Sub

End Class
