"""Minimal Google Play Android Publisher v3 client using a service account.

Credentials: GOOGLE_PLAY_SA points to the service-account JSON (never in git).
"""
from __future__ import annotations

import json
import os
import time
import urllib.error
import urllib.request

import jwt

BASE = "https://androidpublisher.googleapis.com/androidpublisher/v3/applications"
UPLOAD = "https://androidpublisher.googleapis.com/upload/androidpublisher/v3/applications"
_token: dict = {}


class PlayError(RuntimeError):
    pass


def token() -> str:
    if _token.get("exp", 0) > time.time() + 60:
        return _token["value"]
    sa = json.load(open(os.environ["GOOGLE_PLAY_SA"]))
    now = int(time.time())
    assertion = jwt.encode({"iss": sa["client_email"], "scope": "https://www.googleapis.com/auth/androidpublisher",
                            "aud": "https://oauth2.googleapis.com/token", "iat": now, "exp": now + 3600},
                           sa["private_key"], algorithm="RS256")
    body = f"grant_type=urn%3Aietf%3Aparams%3Aoauth%3Agrant-type%3Ajwt-bearer&assertion={assertion}".encode()
    with urllib.request.urlopen(urllib.request.Request("https://oauth2.googleapis.com/token", data=body)) as r:
        tok = json.loads(r.read())
    _token.update(value=tok["access_token"], exp=now + tok["expires_in"])
    return _token["value"]


def call(method: str, url: str, body: dict | None = None, raw: bytes | None = None,
         content_type: str = "application/json") -> dict:
    data = raw if raw is not None else (json.dumps(body).encode() if body is not None else None)
    req = urllib.request.Request(url, data=data, method=method,
                                 headers={"Authorization": "Bearer " + token(), "Content-Type": content_type})
    try:
        with urllib.request.urlopen(req, timeout=600) as r:
            text = r.read()
            return json.loads(text) if text else {}
    except urllib.error.HTTPError as err:
        raise PlayError(f"{method} {url.split('/applications/')[-1]} -> {err.code}: "
                        f"{err.read().decode(errors='replace')[:600]}") from None
