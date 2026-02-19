namespace ula
{
    public interface IUlaBus
    {
        // Returns keyboard state for the given half-row selection mask.
        // highByte is the high byte of the port address (selects keyboard half-rows).
        // Returns bits 0-4 active low (0 = pressed).
        byte ReadKeyboard(byte highByte);

        // EAR input state (bit 6 of port read)
        bool EarInput { get; }
    }
}
