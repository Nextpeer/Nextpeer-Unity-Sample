//////////////////////////////////////////////////////////////
// LaunchIntro.js
// Penelope iPhone Tutorial
//
// LaunchIntro marhsals the separate elements that compose the
// introductory sequence to the game.
//////////////////////////////////////////////////////////////

var spawnParticleEmitter : ParticleEmitter;	
var rumbleSound : AudioClip;
var boomSound : AudioClip;
private var thisTransform : Transform;
private var thisAudio : AudioSource;

function Start() 
{
	// Cache component lookups at startup instead of doing this every frame
	thisTransform = transform;
	thisAudio = audio;

	// Play the rumble sound, which leads up to the boom
	thisAudio.PlayOneShot( rumbleSound, 1.0 );
	
	// Repeatedly shake the camera randomly until the boom
	InvokeRepeating( "CameraShake", 0, 0.05 );
	
	// Launch the particles after the rumble sound has completed
	Invoke( "Launch", rumbleSound.length );
}

function CameraShake() 
{	
	// Pick a random rotation to shake the camera
	var eulerAngles = Vector3( Random.Range( 0, 5 ), Random.Range( 0, 5 ), 0 );
	thisTransform.localEulerAngles = eulerAngles;
}

function Launch()
{
	// Launch the (fake) particles, play the boom sound and cancel any further
	// camera shaking
	spawnParticleEmitter.emit = true;
	thisAudio.PlayOneShot( boomSound, 1.0 );
	Invoke( "CancelInvoke", 0.5 );
}