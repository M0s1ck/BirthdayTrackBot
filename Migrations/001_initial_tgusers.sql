CREATE TABLE IF NOT EXISTS tgusers
(
    id       INT AUTO_INCREMENT
        PRIMARY KEY,
    username VARCHAR(100)                                               NULL,
    birthday DATE                                                       NULL,
    chatId   BIGINT                                                     NULL,
    state    ENUM ('EnteringBirthday', 'Updating', 'Deleting', 'Added') NOT NULL,
    room     ENUM ('Fam', 'HseMates', 'OldMates')                       NOT NULL
);


