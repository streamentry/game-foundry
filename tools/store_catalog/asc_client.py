"""Minimal App Store Connect API client (ES256 JWT, JSON:API).

Credentials come from the environment, never from the repository:
ASC_KEY_ID, ASC_ISSUER_ID and ASC_KEY_PATH (the .p8 file).
"""
from __future__ import annotations

import json
import os
import time
import urllib.error
import urllib.request

import jwt

BASE = "https://api.appstoreconnect.apple.com"


class AscError(RuntimeError):
    pass


def _token() -> str:
    now = int(time.time())
    key = open(os.environ["ASC_KEY_PATH"]).read()
    return jwt.encode({"iss": os.environ["ASC_ISSUER_ID"], "iat": now, "exp": now + 1100,
                       "aud": "appstoreconnect-v1"}, key, algorithm="ES256",
                      headers={"kid": os.environ["ASC_KEY_ID"]})


def request(method: str, path: str, body: dict | None = None, ok=(200, 201, 204)) -> dict:
    url = path if path.startswith("http") else BASE + path
    data = json.dumps(body).encode() if body is not None else None
    req = urllib.request.Request(url, method=method, data=data,
                                 headers={"Authorization": "Bearer " + _token(), "Content-Type": "application/json"})
    try:
        with urllib.request.urlopen(req, timeout=120) as resp:
            raw = resp.read()
            return json.loads(raw) if raw else {}
    except urllib.error.HTTPError as err:
        detail = err.read().decode(errors="replace")
        if err.code in ok:
            return json.loads(detail) if detail else {}
        raise AscError(f"{method} {path} -> {err.code}: {detail[:800]}") from None


def get(path: str) -> dict:
    return request("GET", path)


def post(type_: str, path: str, attributes: dict, relationships: dict | None = None) -> dict:
    data = {"type": type_, "attributes": attributes}
    if relationships:
        data["relationships"] = relationships
    return request("POST", path, {"data": data})


def patch(type_: str, path: str, id_: str, attributes: dict | None = None, relationships: dict | None = None) -> dict:
    data: dict = {"type": type_, "id": id_}
    if attributes:
        data["attributes"] = attributes
    if relationships:
        data["relationships"] = relationships
    return request("PATCH", path, {"data": data})


def rel(type_: str, id_: str) -> dict:
    return {"data": {"type": type_, "id": id_}}
