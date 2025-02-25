CREATE TABLE IF NOT EXISTS invitationcodes
(
    id      INT AUTO_INCREMENT
        PRIMARY KEY,
    code    INT                                  NOT NULL,
    inviter VARCHAR(50)                          NOT NULL,
    room    ENUM ('Fam', 'HseMates', 'OldMates') NOT NULL
);


