export async function copySecret(secret) {
    await navigator.clipboard.writeText(secret);
}
