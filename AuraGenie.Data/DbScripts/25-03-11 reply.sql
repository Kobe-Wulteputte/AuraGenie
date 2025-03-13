CREATE TABLE Messages_dg_tmp
(
    Id             INTEGER
        PRIMARY KEY autoincrement,
    MessageContent TEXT    NOT NULL,
    CreatedOn      REAL    NOT NULL,
    SenderId       TEXT    NOT NULL,
    RoomId         INTEGER NOT NULL,
    ReplyMessageId integer
        CONSTRAINT Messages_Messages_Id_fk
            REFERENCES Messages
);

INSERT INTO Messages_dg_tmp(Id, MessageContent, CreatedOn, SenderId, RoomId)
SELECT Id, MessageContent, CreatedOn, SenderId, RoomId
FROM Messages;

DROP TABLE Messages;

ALTER TABLE Messages_dg_tmp
    rename TO Messages;

create table AuraPointsLog_dg_tmp
(
    Id              INTEGER
        primary key autoincrement,
    UserId          INTEGER not null
        references Users,
    Points          INTEGER not null,
    CreatedOn       REAL    not null,
    SourceMessageId integer
        constraint AuraPointsLog_Messages_Id_fk
            references Messages
);

insert into AuraPointsLog_dg_tmp(Id, UserId, Points, CreatedOn)
select Id, UserId, Points, CreatedOn
from AuraPointsLog;

drop table AuraPointsLog;

alter table AuraPointsLog_dg_tmp
    rename to AuraPointsLog;

