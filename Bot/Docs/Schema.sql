/* Move to the database that was automatically created */
\c nerdbot

/* Add UUID extension */
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

/* Create schemas */
CREATE SCHEMA IF NOT EXISTS config;
CREATE SCHEMA IF NOT EXISTS reply;
CREATE SCHEMA IF NOT EXISTS filter;
CREATE SCHEMA IF NOT EXISTS reactions;
CREATE SCHEMA IF NOT EXISTS logs;

/* Create tables */
CREATE TABLE logs.commands
(
    id         UUID               DEFAULT uuid_generate_v4(),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    user_id    NUMERIC   NOT NULL,
    guild_id   NUMERIC   NOT NULL,
    command    TEXT      NOT NULL,
    args       TEXT[]    NOT NULL,
    PRIMARY KEY (id)
);

/* Used for storing persistent config data that should change and be cross-language */
CREATE TABLE config.data
(
    id         UUID               DEFAULT uuid_generate_v4(),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    key        TEXT      NOT NULL UNIQUE,
    value      TEXT,
    PRIMARY KEY (id)
);

CREATE TABLE public.users
(
    id               NUMERIC   NOT NULL, /* Discord ID */
    created_at       TIMESTAMP NOT NULL DEFAULT NOW(),
    username         TEXT      NOT NULL,
    banned           BOOLEAN   NOT NULL DEFAULT FALSE,
    message_tracking BOOLEAN   NOT NULL DEFAULT TRUE,
    admin            BOOLEAN   NOT NULL DEFAULT FALSE,
    PRIMARY KEY (id)
);

CREATE TABLE public.guilds
(
    id               NUMERIC   NOT NULL, /* Discord ID */
    created_at       TIMESTAMP NOT NULL DEFAULT NOW(),
    name             TEXT      NOT NULL,
    message_tracking BOOLEAN   NOT NULL DEFAULT TRUE,
    PRIMARY KEY (id)
);

CREATE TABLE public.guilds_users
(
    id               UUID               DEFAULT uuid_generate_v4(),
    created_at       TIMESTAMP NOT NULL DEFAULT NOW(),
    user_id          NUMERIC   NOT NULL,
    guild_id         NUMERIC   NOT NULL,
    message_tracking BOOLEAN   NOT NULL DEFAULT TRUE,
    messages_sent    NUMERIC   NOT NULL DEFAULT 0,
    PRIMARY KEY (id)
);

CREATE TABLE public.channels
(
    id               NUMERIC   NOT NULL,
    created_at       TIMESTAMP NOT NULL DEFAULT NOW(),
    guild_id         NUMERIC,
    message_tracking BOOLEAN   NOT NULL DEFAULT TRUE,
    name             TEXT      NOT NULL,
    type             TEXT      NOT NULL,
    PRIMARY KEY (id)
);

CREATE TABLE public.channels_users
(
    id               UUID               DEFAULT uuid_generate_v4(),
    created_at       TIMESTAMP NOT NULL DEFAULT NOW(),
    user_id          NUMERIC   NOT NULL,
    channel_id       NUMERIC   NOT NULL,
    message_tracking BOOLEAN   NOT NULL DEFAULT TRUE,
    messages_sent    BIGINT    NOT NULL DEFAULT 0,
    PRIMARY KEY (id)
);


/*
What I want to be able to do:
- Toggle replies matching specific regexes for all replies in a guild,
- Toggle replies matching specific regexes for all replies from a specific user in a specific guild
- Toggle replies matching specific regexes for all replies from a specific user in all guilds
*/
CREATE TABLE reply.filters
(
    id         UUID             DEFAULT uuid_generate_v4(),
    applies_to NUMERIC NOT NULL, /* User who's replies are filtered */
    guild_id   NUMERIC,
    channel_id NUMERIC,
    user_id    NUMERIC,
    regex      TEXT    NOT NULL,
    enabled    BOOLEAN NOT NULL DEFAULT TRUE,
    PRIMARY KEY (id)
);

/* What I want to be able to do
- Automatically delete messages that match a specific regex globally
- Automatically delete messages that match a specific regex in a specific guild
- Automatically delete messages that match a specific regex from a specific user in a specific guild
- Automatically delete messages that match a specific regex from a specific user in all guilds
*/

CREATE TABLE filter.filters
(
    id         UUID               DEFAULT uuid_generate_v4(),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    guild_id   NUMERIC,
    channel_id NUMERIC,
    user_id    NUMERIC,
    regex      TEXT      NOT NULL,
    enabled    BOOLEAN   NOT NULL DEFAULT TRUE,
    PRIMARY KEY (id)
);


CREATE TABLE reactions.reactions
(
    id         UUID               DEFAULT uuid_generate_v4(),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    guild_id   NUMERIC,
    channel_id NUMERIC,
    user_id    NUMERIC   NOT NULL,
    emoji      TEXT,
    emoji_id   NUMERIC,
    type       TEXT      NOT NULL,
    PRIMARY KEY (id)
);

/* Add Multi-Null Constraints */
ALTER TABLE reactions.reactions
    ADD CONSTRAINT check_reactions_reactions_emoji_or_emoji_id_not_null CHECK (emoji IS NOT NULL OR emoji_id IS NOT NULL);

ALTER TABLE reactions.reactions
    ADD CONSTRAINT check_reactions_reactions_emoji_or_emoji_id_null CHECK (emoji IS NULL OR emoji_id IS NULL);

/* Create views */
CREATE VIEW channel_message_view AS
SELECT g.id             AS guild_id,
       c.id             AS channel_id,
       u.id             AS user_id,
       g.name           AS guildname,
       c.name           AS channelname,
       u.username       AS username,
       cu.messages_sent AS messages_sent

FROM channels_users AS cu
         JOIN public.users u ON cu.user_id = u.id
         JOIN public.channels c ON cu.channel_id = c.id
         JOIN public.guilds g ON c.guild_id = g.id
;

CREATE VIEW guild_message_view AS
SELECT g.id                  AS guild_id,
       u.id                  AS user_id,
       g.name                AS guild_name,
       u.username            as user_name,
       sum(cu.messages_sent) AS messages_sent

FROM channels_users AS cu
         JOIN public.users u ON cu.user_id = u.id
         JOIN public.channels c ON cu.channel_id = c.id
         JOIN public.guilds g ON c.guild_id = g.id
GROUP BY u.id, g.id
;

CREATE VIEW global_message_view AS
SELECT u.id                  AS user_id,
       u.username            AS user_name,
       sum(cu.messages_sent) AS messages_sent

FROM channels_users AS cu
         JOIN public.users u ON cu.user_id = u.id
GROUP BY u.id;


/* 
Create indexes 

Note: Creating primary key indexes is done automatically when creating the table
*/
CREATE INDEX idx_commands_user_id ON logs.commands (user_id);
CREATE INDEX idx_commands_guild_id ON logs.commands (guild_id);

CREATE INDEX idx_users_banned ON public.users (banned);
CREATE INDEX idx_users_message_tracking ON public.users (message_tracking);
CREATE INDEX idx_users_admin ON public.users (admin);

CREATE INDEX idx_guilds_message_tracking ON public.guilds (message_tracking);

CREATE INDEX idx_guilds_channels_channel_id ON public.channels (id);
CREATE INDEX idx_guilds_channels_guild_id ON public.channels (guild_id);
CREATE INDEX idx_guilds_channels_message_tracking ON public.channels (message_tracking);
CREATE INDEX idx_guilds_channels_type ON public.channels (type);

CREATE INDEX idx_guilds_users_user_id ON public.guilds_users (user_id);
CREATE INDEX idx_guilds_users_guild_id ON public.guilds_users (guild_id);
CREATE INDEX idx_guilds_users_message_tracking ON public.guilds_users (message_tracking);

CREATE INDEX idx_channels_users_user_id ON public.channels_users (user_id);
CREATE INDEX idx_channels_users_channel_id ON public.channels_users (channel_id);
CREATE INDEX idx_channels_users_message_tracking ON public.channels_users (message_tracking);
CREATE INDEX idx_channels_users_messages_sent ON public.channels_users (messages_sent);

CREATE INDEX idx_reply_filters_applies_to ON reply.filters (applies_to);
CREATE INDEX idx_reply_filters_guild_id ON reply.filters (guild_id);
CREATE INDEX idx_reply_filters_user_id ON reply.filters (user_id);

CREATE INDEX idx_filter_filters_guild_id ON filter.filters (guild_id);
CREATE INDEX idx_filter_filters_user_id ON filter.filters (user_id);

CREATE INDEX idx_reactions_guild_id ON reactions.reactions (guild_id);
CREATE INDEX idx_reactions_channel_id ON reactions.reactions (channel_id);
CREATE INDEX idx_reactions_user_id ON reactions.reactions (user_id);

/* Create Foreign Keys */
ALTER TABLE logs.commands
    ADD FOREIGN KEY (user_id) REFERENCES public.users (id) ON DELETE CASCADE;
ALTER TABLE logs.commands
    ADD FOREIGN KEY (guild_id) REFERENCES public.guilds (id) ON DELETE CASCADE;

ALTER TABLE public.guilds_users
    ADD FOREIGN KEY (user_id) REFERENCES public.users (id) ON DELETE CASCADE;
ALTER TABLE public.guilds_users
    ADD FOREIGN KEY (guild_id) REFERENCES public.guilds (id) ON DELETE CASCADE;

ALTER TABLE public.channels
    ADD FOREIGN KEY (guild_id) REFERENCES public.guilds (id) ON DELETE CASCADE;

ALTER TABLE public.channels_users
    ADD FOREIGN KEY (user_id) REFERENCES public.users (id) ON DELETE CASCADE;
ALTER TABLE public.channels_users
    ADD FOREIGN KEY (channel_id) REFERENCES public.channels (id) ON DELETE CASCADE;



ALTER TABLE filter.filters
    ADD FOREIGN KEY (guild_id) REFERENCES public.guilds (id) ON DELETE CASCADE;
ALTER TABLE filter.filters
    ADD FOREIGN KEY (channel_id) REFERENCES public.channels (id) ON DELETE CASCADE;
ALTER TABLE filter.filters
    ADD FOREIGN KEY (user_id) REFERENCES public.users (id) ON DELETE CASCADE;

ALTER TABLE reactions.reactions
    ADD FOREIGN KEY (guild_id) REFERENCES public.guilds (id) ON DELETE CASCADE;
ALTER TABLE reactions.reactions
    ADD FOREIGN KEY (channel_id) REFERENCES public.channels (id) ON DELETE CASCADE;
ALTER TABLE reactions.reactions
    ADD FOREIGN KEY (user_id) REFERENCES public.users (id) ON DELETE CASCADE;

ALTER TABLE reply.filters
    ADD FOREIGN KEY (applies_to) REFERENCES public.users (id) ON DELETE CASCADE;
ALTER TABLE reply.filters
    ADD FOREIGN KEY (guild_id) REFERENCES public.guilds (id) ON DELETE CASCADE;
ALTER TABLE reply.filters
    ADD FOREIGN KEY (user_id) REFERENCES public.users (id) ON DELETE CASCADE;

/* Grant permissions */
GRANT ALL PRIVILEGES ON DATABASE nerdbot TO nerdbot;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO nerdbot;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA reply TO nerdbot;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA filter TO nerdbot;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA reactions TO nerdbot;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA logs TO nerdbot;
GRANT USAGE ON ALL SEQUENCES IN SCHEMA public TO nerdbot;
GRANT USAGE ON ALL SEQUENCES IN SCHEMA reply TO nerdbot;
GRANT USAGE ON ALL SEQUENCES IN SCHEMA filter TO nerdbot;
GRANT USAGE ON ALL SEQUENCES IN SCHEMA reactions TO nerdbot;
GRANT USAGE ON ALL SEQUENCES IN SCHEMA logs TO nerdbot;

/* Add autofill data */
INSERT INTO public.users (id, username)
VALUES (1, 'System');
INSERT INTO public.guilds (id, name)
VALUES (1, 'System');
INSERT INTO public.channels (id, guild_id, name, type)
VALUES (1, 1, 'System', 'System');
