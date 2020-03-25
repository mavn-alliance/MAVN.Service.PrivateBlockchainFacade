MERGE private_blockchain_facade.nonce_counters WITH (HOLDLOCK) AS target
    USING (
            SELECT @masterWalletAddress
            ) AS source(master_wallet_address)
        ON (
            target.master_wallet_address = source.master_wallet_address
        )
        WHEN MATCHED
        THEN
UPDATE
    SET counter_value = counter_value + 1
    WHEN NOT MATCHED
    THEN
INSERT (master_wallet_address, counter_value)
VALUES (source.master_wallet_address, 0)
    OUTPUT inserted.master_wallet_address, inserted.counter_value;