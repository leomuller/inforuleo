CREATE TABLE lt_status (
    status_id   SMALLINT PRIMARY KEY,
    status_name VARCHAR(50) NOT NULL
);

INSERT INTO lt_status (status_id, status_name) VALUES
    (2, 'initializing'),
    (3, 'active'),
    (4, 'archived');
-- 1 is intentionally skipped, so it's never mistaken for a boolean.

CREATE TABLE url_mappings (
    mapping_id           BIGSERIAL PRIMARY KEY,
    url_short            VARCHAR(50)  NULL,
    url_long             TEXT         NOT NULL,
    url_long_normalized  TEXT         NOT NULL,
    date_created         TIMESTAMPTZ  NOT NULL,
    date_last_used       TIMESTAMPTZ  NULL,
    status_id            SMALLINT     NOT NULL REFERENCES lt_status (status_id)
);

CREATE UNIQUE INDEX ux_url_mappings_url_long_normalized ON url_mappings (url_long_normalized);
CREATE UNIQUE INDEX ux_url_mappings_url_short ON url_mappings (url_short) WHERE url_short IS NOT NULL;

-- Tables above were created by the postgres superuser (that's who runs these init scripts) -
-- apiuser needs explicit grants to read/write them, and to use the BIGSERIAL's underlying sequence.
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO apiuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO apiuser;
