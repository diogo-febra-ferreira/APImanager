CREATE TABLE Resources
(
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [res_type]    NVARCHAR (30)  NOT NULL,
    [name]        NVARCHAR (100),
    [creation_dt] NVARCHAR (100) NOT NULL,
    [parent]      INT            NULL,
    [event]       NVARCHAR (100) NULL,
    [endpoint]    NVARCHAR (100) NULL,
    [content]     NVARCHAR (500) NULL,
)