"""Add short-lived browser login sessions.

Revision ID: 0002_discord_login
Revises: 0001_initial
Create Date: 2026-07-26
"""

from collections.abc import Sequence

from alembic import op
import sqlalchemy as sa


revision: str = "0002_discord_login"
down_revision: str | None = "0001_initial"
branch_labels: str | Sequence[str] | None = None
depends_on: str | Sequence[str] | None = None


def upgrade() -> None:
    op.create_table(
        "device_login_sessions",
        sa.Column("id", sa.Integer(), nullable=False),
        sa.Column("user_id", sa.Integer(), nullable=True),
        sa.Column(
            "provider",
            sa.String(length=32),
            server_default="discord",
            nullable=False,
        ),
        sa.Column("device_code_hash", sa.String(length=64), nullable=False),
        sa.Column("browser_code_hash", sa.String(length=64), nullable=False),
        sa.Column("oauth_state_hash", sa.String(length=64), nullable=True),
        sa.Column(
            "status",
            sa.String(length=16),
            server_default="pending",
            nullable=False,
        ),
        sa.Column("failure_code", sa.String(length=64), nullable=True),
        sa.Column(
            "created_at",
            sa.DateTime(timezone=True),
            server_default=sa.func.now(),
            nullable=False,
        ),
        sa.Column("expires_at", sa.DateTime(timezone=True), nullable=False),
        sa.Column("authorized_at", sa.DateTime(timezone=True), nullable=True),
        sa.Column("consumed_at", sa.DateTime(timezone=True), nullable=True),
        sa.CheckConstraint(
            "status IN ('pending', 'authorized', 'consumed', 'failed', 'expired')",
            name="ck_device_login_sessions_valid_status",
        ),
        sa.ForeignKeyConstraint(
            ["user_id"],
            ["users.id"],
            name="fk_device_login_sessions_user_id_users",
            ondelete="CASCADE",
        ),
        sa.PrimaryKeyConstraint("id", name="pk_device_login_sessions"),
        sa.UniqueConstraint(
            "browser_code_hash",
            name="uq_device_login_sessions_browser_code_hash",
        ),
        sa.UniqueConstraint(
            "device_code_hash",
            name="uq_device_login_sessions_device_code_hash",
        ),
        sa.UniqueConstraint(
            "oauth_state_hash",
            name="uq_device_login_sessions_oauth_state_hash",
        ),
    )
    op.create_index(
        "ix_device_login_sessions_expires_at",
        "device_login_sessions",
        ["expires_at"],
    )
    op.create_index(
        "ix_device_login_sessions_user_id",
        "device_login_sessions",
        ["user_id"],
    )


def downgrade() -> None:
    op.drop_index(
        "ix_device_login_sessions_user_id",
        table_name="device_login_sessions",
    )
    op.drop_index(
        "ix_device_login_sessions_expires_at",
        table_name="device_login_sessions",
    )
    op.drop_table("device_login_sessions")
