CREATE OR ALTER PROCEDURE sp_ApproveOrder
    @OrderId INT,
    @PerformedBy NVARCHAR(100),
    @NewStatus INT,
    @Comments NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Check if order exists and is still pending
        IF EXISTS (SELECT 1 FROM Orders WHERE Id = @OrderId AND Status = 0)
        BEGIN
            -- Update Order Status
            UPDATE Orders 
            SET Status = @NewStatus 
            WHERE Id = @OrderId;

            -- Insert Audit Log
            INSERT INTO AuditLogs (OrderId, Action, PerformedBy, Timestamp, Comments)
            VALUES (@OrderId, 
                    CASE WHEN @NewStatus = 1 THEN 'Approved' ELSE 'Rejected' END, 
                    @PerformedBy, 
                    GETUTCDATE(), 
                    @Comments);

            COMMIT TRANSACTION;
            SELECT 1 AS Success, 'Order processed successfully.' AS Message;
        END
        ELSE
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Success, 'Order not found or already processed.' AS Message;
        END
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 AS Success, ERROR_MESSAGE() AS Message;
    END CATCH
END
