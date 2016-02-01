Imports System.Management

Friend Class RegistryKeyChangeEventArgs
    Inherits EventArgs

    Public Property Hive() As String
    Public Property KeyPath() As String
    Public Property SECURITY_DESCRIPTOR() As UInteger()
    Public Property TIME_CREATED() As Date

    Public Sub New(ByVal arrivedEvent As ManagementBaseObject)

        Me.Hive = TryCast(arrivedEvent.Properties("Hive").Value, String)
        Me.KeyPath = TryCast(arrivedEvent.Properties("KeyPath").Value, String)

        Me.TIME_CREATED = New Date(
            CLng(Fix(CULng(arrivedEvent.Properties("TIME_CREATED").Value))),
            DateTimeKind.Utc).AddYears(1600)
    End Sub
End Class
