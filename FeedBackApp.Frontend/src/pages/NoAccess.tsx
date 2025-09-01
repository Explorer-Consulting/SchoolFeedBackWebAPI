export default function NoAccess() {
  return (
    <div className="min-h-screen flex items-center justify-center bg-background">
      <div className="text-center">
        <h1 className="text-3xl font-bold mb-4">Sorry,you do not have access!</h1>
        <a href="/" className="underline text-primary hover:opacity-80">
          Return to Home
        </a>
      </div>
    </div>
  );
}